using AngleSharp.Html.Parser;
using BrokenLinkChecker.Domain;
using BrokenLinkChecker.LinkChecker.Core.Models;
using System.Collections.Concurrent;

namespace BrokenLinkChecker.LinkChecker.Core
{

    public class LinkCheckerEngine
    {
        public int MaxDegreeOfParallelism { get; set; }

        public List<PageSite> Sites { get; set; }
        private HttpClient cli;

        public bool Purchased { get; set; }
        private Random rnd;
        public int DelayMax { get; set; }
        public int DelayMin { get; set; }
        public LinkCheckerEngine(List<PageSite> Sites, int MaxDegreeOfParallelism, string UserAgent, bool purchased, int delayMax, int delayMin)
        {
            this.Sites = Sites;
            this.MaxDegreeOfParallelism = MaxDegreeOfParallelism;
            cli = new HttpClient();
            if (!string.IsNullOrEmpty(UserAgent))
                cli.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            else
            {
                RandomUserAgentGenerator gen = new RandomUserAgentGenerator();
                cli.DefaultRequestHeaders.UserAgent.ParseAdd(gen.GetRandomUserAgent());
            }
            Purchased = purchased;
            rnd = new Random();
            DelayMax = delayMax;
            DelayMin = delayMin;
        }

        public async Task<List<Domain.PageLink>> CheckLinks(IProgress<PageLink> progress, CancellationToken token)
        {
            var visitedLinks = new ConcurrentDictionary<string, LinkInfo>();
            var pendingLinks = new ConcurrentQueue<LinkInfo>();
            List<PageLink> PageLinks = new List<PageLink>();
            foreach (var site in Sites)
            {
                pendingLinks.Enqueue(new LinkInfo() { LinkText = site.SiteUrl, LinkUrl = site.SiteUrl });
            }

            var semaphore = new SemaphoreSlim(MaxDegreeOfParallelism);
            var tasks = new List<Task>();

            while (pendingLinks.Count > 0 || tasks.Any())
            {
                if (token.IsCancellationRequested)
                    break;
                while (pendingLinks.Count > 0 && tasks.Count < MaxDegreeOfParallelism)
                {
                    if (pendingLinks.TryDequeue(out var currentUrl))
                    {
                        if (!visitedLinks.ContainsKey(currentUrl.LinkUrl))
                        {
                            tasks.Add(ProcessLink(currentUrl, visitedLinks, pendingLinks, semaphore,  PageLinks, token, progress));
                        }

                    }
                    if (token.IsCancellationRequested)
                        break;
                    if (!Purchased)
                    {
                        if (visitedLinks.Count > 100)
                        {
                            break;
                        }
                    }
                }
                if (tasks.Any())
                {
                    await Task.WhenAny(tasks.ToArray());
                    tasks.RemoveAll(t => t.IsCompleted);

                }

            }

            return (PageLinks);
        }



        private async Task ProcessLink(LinkInfo currentUrl, ConcurrentDictionary<string, LinkInfo> visitedLinks, ConcurrentQueue<LinkInfo> pendingLinks, SemaphoreSlim semaphore,
            List<PageLink> PageLinks, CancellationToken cancellationToken, IProgress<PageLink> progress)
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                if (!Purchased)
                {
                    if (visitedLinks.Count > 100)
                    {
                        return;
                    }
                }
                var isBroken = await CheckLinkValidity(currentUrl);
                currentUrl.Broken = isBroken;
                var added = visitedLinks.TryAdd(currentUrl.LinkUrl, currentUrl);

                if (!PageLinks.Any(d => d.LinkUrl == currentUrl.LinkUrl))
                {
                    PageLinks.Add(new PageLink { LinkUrl = currentUrl.LinkUrl, Broken = isBroken, PageName = currentUrl.PageName, LinkText = currentUrl.LinkText });
                    progress.Report(PageLinks.Last());
                    await Task.Delay(10);
                }



                if (!isBroken)
                {
                    var html = await cli.GetStringAsync(currentUrl.LinkUrl);
                    var parser = new HtmlParser();
                    var document = await parser.ParseDocumentAsync(html);

                    var links = document.QuerySelectorAll("a[href]").Distinct().ToList();                    

                    if (links != null)
                    {
                        foreach (var link in links)
                        {
                            if (cancellationToken.IsCancellationRequested)
                                break;
                            var href = link.GetAttribute("href");

                            if (!string.IsNullOrEmpty(href) && !href.Contains("javascript") && !href.Contains("mailto:") && !href.Contains("tel:") )
                            {
                                Uri baseUri = new Uri(currentUrl.LinkUrl);
                                Uri absoluteUri = new Uri(baseUri, href);

                                var absoluteUrl = absoluteUri.ToString();
                                var linkText = link.TextContent;

                                if (!visitedLinks.ContainsKey(absoluteUrl))
                                {
                                    if (IsExternalLink(currentUrl.LinkUrl, absoluteUrl))
                                    {
                                        // Solo verifica, no agrega a la cola para rastreo
                                        var isExternalBroken = await CheckLinkValidity(new LinkInfo() { LinkUrl = absoluteUrl, LinkText = linkText, PageName = currentUrl.LinkUrl });
                                        visitedLinks.TryAdd(absoluteUrl, new LinkInfo() { LinkUrl = absoluteUrl, LinkText = linkText, PageName = currentUrl.LinkUrl, Broken = isExternalBroken });
                                            PageLinks.Add(new PageLink { LinkUrl = absoluteUrl, LinkText = linkText, PageName = currentUrl.LinkUrl, Broken = isExternalBroken });
                                            progress.Report(PageLinks.Last());
                                            await Task.Delay(10);
                                    }
                                    else if (!visitedLinks.ContainsKey(absoluteUrl))
                                    {
                                        pendingLinks.Enqueue(new LinkInfo() { LinkUrl = absoluteUrl, LinkText = linkText, PageName = currentUrl.LinkUrl });
                                    }
                                }


                            }

                        }
                    }
                }

            }

            finally
            {
                semaphore.Release();
            }
        }

        private async Task<bool> CheckLinkValidity(LinkInfo url)
        {
            try
            {
                int sec = rnd.Next(DelayMin, DelayMax) * 1000;
                await Task.Delay(sec);
                var response = await cli.GetAsync(url.LinkUrl);
                return !response.IsSuccessStatusCode;
            }
            catch (Exception)
            {
                return true; // Si hay un error, asumimos que el enlace está roto
            }
        }

        private bool IsExternalLink(string baseUrl, string href)
        {
            Uri baseUri = new Uri(baseUrl);
            Uri absoluteUri = new Uri(href, UriKind.RelativeOrAbsolute);

            // Verifica si la URL es relativa o tiene un dominio diferente
            if (absoluteUri.IsAbsoluteUri)
            {
                string baseHost = baseUri.Host;
                string absoluteHost = absoluteUri.Host;

                // Verifica si los dominios principales son diferentes
                if (!baseHost.Equals(absoluteHost, StringComparison.OrdinalIgnoreCase))
                {
                    // Si los dominios principales son diferentes, devuelve true (es una URL externa)
                    return true;
                }

                // Si los dominios principales son iguales, también verifica los subdominios
                string[] baseSegments = baseUri.Host.Split('.');
                string[] absoluteSegments = absoluteUri.Host.Split('.');

                if (baseSegments.Length > 1 && absoluteSegments.Length > 1)
                {
                    // Compara los subdominios, ignorando el dominio principal
                    for (int i = 1; i < baseSegments.Length && i < absoluteSegments.Length; i++)
                    {
                        if (!baseSegments[i].Equals(absoluteSegments[i], StringComparison.OrdinalIgnoreCase))
                        {
                            // Subdominios diferentes, devuelve false (es una URL interna)
                            return false;
                        }
                    }
                }
            }

            // La URL es relativa o tiene el mismo dominio (incluidos subdominios), devuelve false (es una URL interna)
            return false;
        }
    }

}
