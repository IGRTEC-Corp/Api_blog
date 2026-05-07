#nullable disable

using Api_blog.Model;
using Api_bolsatrabajo.Data;
using Api_bolsatrabajo.Model;
using Api_bolsatrabajo.Model.Dtos;
using BolsaDeTrabajo.Api.DTOs;
using BolsaDeTrabajo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Api_blog.Controllers
{
    [ApiController]
    [Route("api/blog")]
    public class BlogController : ControllerBase
    {
        private readonly BolsatrabajoContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<BlogController> _logger;

        // Instancia unica para evitar Socket Exhaustion
        private static readonly HttpClient _httpClient = new HttpClient();

        private readonly string _serpApiKey;
        private readonly string _pexelsKey;
        private readonly string _groqApiKey;

        // Gestion de tokens en memoria
        private static string _activeFbUserToken = null;
        private static string _activeFbPageToken = null;

        public BlogController(BolsatrabajoContext context, IConfiguration config, ILogger<BlogController> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;

            _serpApiKey = _config["ExternalApis:SerpApi"];
            _pexelsKey = _config["ExternalApis:Pexels"];
            _groqApiKey = _config["ExternalApis:GroqApi"];

            // Inicializacion de tokens desde el archivo de configuracion
            if (string.IsNullOrEmpty(_activeFbUserToken)) _activeFbUserToken = _config["Facebook:UserToken"];
            if (string.IsNullOrEmpty(_activeFbPageToken)) _activeFbPageToken = _config["Facebook:PageToken"];
        }

        // =========================
        // SEGURIDAD LIGERA (API KEY)
        // =========================
        private bool IsAuthorized()
        {
            var secretApiKey = _config["CronJobSecurity:ApiKey"];
            var extractedApiKey = Request.Headers["X-API-KEY"].FirstOrDefault();
            return !string.IsNullOrEmpty(extractedApiKey) && extractedApiKey == secretApiKey;
        }

        // =========================
        // ENDPOINTS PUBLICOS (Lectura Segura)
        // =========================

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.BlogPosts
                .AsNoTracking()
                .Where(p => p.IsPublished)
                .OrderByDescending(p => p.PublishedAt)
                .ToListAsync();
            return Ok(posts);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug)) return BadRequest("Slug invalido");
            var post = await _context.BlogPosts
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

            if (post == null) return NotFound();
            return Ok(post);
        }

        [HttpPost("{slug}/view")]
        public async Task<IActionResult> RecordView(string slug)
        {
            var post = await _context.BlogPosts.FirstOrDefaultAsync(p => p.Slug == slug);
            if (post != null)
            {
                post.Views++;
                await _context.SaveChangesAsync();
            }
            return Ok();
        }

        // =========================
        // ENDPOINTS ADMINISTRATIVOS (Protegidos)
        // =========================

        [HttpGet("admin")]
        public async Task<IActionResult> GetAdmin()
        {
            if (!IsAuthorized()) return Unauthorized(new { error = "Acceso denegado." });
            var posts = await _context.BlogPosts.OrderByDescending(p => p.CreatedAt).ToListAsync();
            return Ok(posts);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BlogPost model)
        {
            if (!IsAuthorized()) return Unauthorized(new { error = "Acceso denegado." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            model.Slug = GenerateSlug(model.Title);
            model.CreatedAt = DateTime.UtcNow;
            model.Views = 0;
            if (model.IsPublished && model.PublishedAt == null) model.PublishedAt = DateTime.UtcNow;

            _context.BlogPosts.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetBySlug), new { slug = model.Slug }, model);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BlogPost model)
        {
            if (!IsAuthorized()) return Unauthorized(new { error = "Acceso denegado." });
            if (id != model.BlogPostId) return BadRequest("Id no coincide");

            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            post.Title = model.Title;
            post.Summary = model.Summary;
            post.Content = model.Content;
            post.MetaTitle = model.MetaTitle;
            post.MetaDescription = model.MetaDescription;
            post.MetaKeywords = model.MetaKeywords;
            post.FeaturedImageUrl = model.FeaturedImageUrl;
            post.AuthorName = model.AuthorName;
            post.IsPublished = model.IsPublished;
            post.PublishedAt = model.IsPublished ? model.PublishedAt ?? DateTime.UtcNow : null;
            post.UpdatedAt = DateTime.UtcNow;
            post.Slug = GenerateSlug(model.Title);

            _context.Update(post);
            await _context.SaveChangesAsync();
            return Ok(post);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (!IsAuthorized()) return Unauthorized(new { error = "Acceso denegado." });
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null) return NotFound();

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // =========================
        // MOTOR 1: TENDENCIAS Y NOTICIAS
        // =========================

        [HttpGet("auto-trend2")]
        public async Task<IActionResult> AutoTrendSectorTech()
        {
            if (!IsAuthorized()) return Unauthorized(new { error = "Acceso denegado." });

            try
            {
                int maxPostsPerDay = 3;
                TimeZoneInfo tz;
                try { tz = TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City"); }
                catch { tz = TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"); }

                DateTime mexicoTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tz);
                var postsToday = await _context.BlogPosts
                    .CountAsync(p => p.CreatedAt >= mexicoTime.Date.ToUniversalTime() && p.CreatedAt < mexicoTime.Date.AddDays(1).ToUniversalTime());

                if (postsToday >= maxPostsPerDay) return Ok(new { success = false, message = "Cupo lleno." });

                var request = new HttpRequestMessage(HttpMethod.Get, $"https://serpapi.com/search.json?q={Uri.EscapeDataString("noticias (lanzamiento OR novedades) ('inteligencia artificial' OR 'LLM' OR 'Nvidia H100' OR 'Claude' OR 'GPT-5' OR 'cybersecurity AI') after:2026-05-01")}&tbm=nws&hl=es&gl=mx&api_key={_serpApiKey}");
                var nResp = await _httpClient.SendAsync(request);

                if (!nResp.IsSuccessStatusCode) return BadRequest("Error al consultar sector");

                var nJson = await nResp.Content.ReadAsStringAsync();
                using var nDoc = JsonDocument.Parse(nJson);
                if (!nDoc.RootElement.TryGetProperty("news_results", out var results)) return Ok("Sin noticias nuevas");

                foreach (var article in results.EnumerateArray())
                {
                    string rawTitle = article.TryGetProperty("title", out var t) ? t.GetString() : "Sin titulo";
                    string rawSnippet = article.TryGetProperty("snippet", out var s) ? s.GetString() : "Novedad tecnologica";
                    string slug = GenerateSlug(rawTitle);

                    if (await _context.BlogPosts.AnyAsync(p => p.Slug == slug)) continue;

                    var checkBody = new
                    {
                        model = "llama-3.1-8b-instant",
                        messages = new[] {
                            new { role = "system", content = "Filtro IGRTEC. Responde solo APTO o DESCARTAR. Acepta temas de IA, infraestructura, servidores y ciberseguridad." },
                            new { role = "user", content = $"Es relevante?: Titulo: {rawTitle}. Contexto: {rawSnippet}." }
                        },
                        temperature = 0.0
                    };

                    var contentReq = new StringContent(JsonSerializer.Serialize(checkBody), Encoding.UTF8, "application/json");
                    var cRespReq = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions") { Content = contentReq };
                    cRespReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);
                    var cResp = await _httpClient.SendAsync(cRespReq);
                    var cJson = await cResp.Content.ReadFromJsonAsync<JsonElement>();
                    string decision = cJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString().ToUpper();

                    if (!decision.Contains("APTO")) continue;
                    return await GenerarYGuardarCompleto(article, slug, rawSnippet);
                }
                return Ok(new { success = false, message = "Sin novedades." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en AutoTrend");
                return StatusCode(500, "Error interno");
            }
        }

        private async Task<IActionResult> GenerarYGuardarCompleto(JsonElement article, string slug, string rawSnippet)
        {
            string title = article.TryGetProperty("title", out var t) ? t.GetString() : "Sin titulo";
            string source = article.TryGetProperty("source", out var src) ? src.GetString() : "Fuente";
            string link = article.TryGetProperty("link", out var l) ? l.GetString() : "https://igrtec.com";

            var groqBody = new
            {
                model = "llama-3.3-70b-versatile",
                messages = new[] {
                    new { role = "system", content = "Analista IGRTEC. Escribe un analisis tecnico sobre como esta novedad impacta servidores y software. Usa HTML." },
                    new { role = "user", content = $"Noticia: {title}. Resumen: {rawSnippet}" }
                },
                temperature = 0.6
            };

            var contentReq = new StringContent(JsonSerializer.Serialize(groqBody), Encoding.UTF8, "application/json");
            var gRespReq = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions") { Content = contentReq };
            gRespReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);
            var gResp = await _httpClient.SendAsync(gRespReq);
            var gJson = await gResp.Content.ReadFromJsonAsync<JsonElement>();
            string aiContent = gJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

            string imageUrl = "https://igrtec.com/images/blog/default-tech.jpg";
            try
            {
                var pexelsReq = new HttpRequestMessage(HttpMethod.Get, "https://api.pexels.com/v1/search?query=artificial+intelligence+technology+cybersecurity&per_page=15");
                pexelsReq.Headers.Add("Authorization", _pexelsKey);
                var imgRes = await _httpClient.SendAsync(pexelsReq);
                if (imgRes.IsSuccessStatusCode)
                {
                    var iJ = await imgRes.Content.ReadFromJsonAsync<JsonElement>();
                    int totalImages = iJ.GetProperty("photos").GetArrayLength();
                    if (totalImages > 0) imageUrl = iJ.GetProperty("photos")[new Random().Next(0, totalImages)].GetProperty("src").GetProperty("large").GetString();
                }
            }
            catch { }

            var post = new BlogPost
            {
                Title = title.Length > 200 ? title.Substring(0, 197) + "..." : title,
                Slug = slug,
                Summary = rawSnippet.Length > 500 ? rawSnippet.Substring(0, 497) + "..." : rawSnippet,
                Content = $"{aiContent}<p>Fuente: <a href='{link}' target='_blank'>{source}</a></p>",
                MetaTitle = title.Length > 200 ? title.Substring(0, 197) + "..." : title,
                MetaDescription = rawSnippet.Length > 300 ? rawSnippet.Substring(0, 297) + "..." : rawSnippet,
                MetaKeywords = "IA, IGRTEC, Infraestructura",
                FeaturedImageUrl = imageUrl,
                IsPublished = true,
                PublishedAt = DateTime.UtcNow,
                AuthorName = "Jose Ramon Orzuna - IGRTEC",
                CreatedAt = DateTime.UtcNow
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            string blogUrl = $"https://igrtec.com/blog/{slug}";
            string socialMsg = $"[IA & INFRAESTRUCTURA - IGRTEC]\n\nNoticia: {title}\n\nAnalisis completo del impacto tecnologico aqui: {blogUrl}\n\n#IGRTEC #AI #Servers";

            try { await PublishToFacebookOptimized(socialMsg, blogUrl); } catch { }
            try { await PublishLinkedIn(GenerateLinkedInMessage("Inteligencia Artificial", title, blogUrl)); } catch { }

            return Ok(new { success = true, slug = post.Slug });
        }

        // =========================
        // MOTOR 2: GENERADOR DE PILARES (SEO MAESTRO)
        // =========================

        [HttpGet("generate-pillar-content")]
        public async Task<IActionResult> GeneratePillarContent()
        {
            if (!IsAuthorized()) return Unauthorized(new { error = "Acceso denegado." });

            try
            {
                string[] masterTopics = {
                    "Ventajas de Servidores Bare Metal vs Nube Publica en 2026",
                    "Guia de Alta Disponibilidad y Redundancia en Datacenters",
                    "Ciberseguridad Perimetral: Firewalls de Proxima Generacion",
                    "Optimizacion de SQL Server en Infraestructura Fisica"
                };

                string selectedTopic = masterTopics[new Random().Next(masterTopics.Length)];

                var groqBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[] {
                        new { role = "system", content = "Eres Director de IGRTEC. Escribe una GUIA MAESTRA de ingenieria. 5 secciones, tabla comparativa HTML, explica por que el bare metal es superior." },
                        new { role = "user", content = $"Guia sobre: {selectedTopic}" }
                    },
                    temperature = 0.5
                };

                var contentReq = new StringContent(JsonSerializer.Serialize(groqBody), Encoding.UTF8, "application/json");
                var gRespReq = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions") { Content = contentReq };
                gRespReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);
                var gResp = await _httpClient.SendAsync(gRespReq);
                var gJson = await gResp.Content.ReadFromJsonAsync<JsonElement>();
                string fullContent = gJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                string imageUrl = "https://igrtec.com/images/blog/pillar-default.jpg";
                try
                {
                    var pexelsReq = new HttpRequestMessage(HttpMethod.Get, "https://api.pexels.com/v1/search?query=server+room+datacenter+technology&per_page=15");
                    pexelsReq.Headers.Add("Authorization", _pexelsKey);
                    var imgRes = await _httpClient.SendAsync(pexelsReq);
                    if (imgRes.IsSuccessStatusCode)
                    {
                        var iJ = await imgRes.Content.ReadFromJsonAsync<JsonElement>();
                        int totalImages = iJ.GetProperty("photos").GetArrayLength();
                        if (totalImages > 0) imageUrl = iJ.GetProperty("photos")[new Random().Next(0, totalImages)].GetProperty("src").GetProperty("large").GetString();
                    }
                }
                catch { }

                var slug = GenerateSlug(selectedTopic);
                if (await _context.BlogPosts.AnyAsync(p => p.Slug == slug)) return Ok("Pilar ya existe.");

                var post = new BlogPost
                {
                    Title = selectedTopic,
                    Slug = slug,
                    Summary = $"Guia tecnica de ingenieria sobre {selectedTopic}.",
                    Content = fullContent,
                    MetaTitle = $"{selectedTopic} | IGRTEC",
                    MetaDescription = $"Guia tecnica detallada de {selectedTopic} para profesionales IT.",
                    MetaKeywords = "Servidores, Datacenter, Bare Metal, IGRTEC",
                    FeaturedImageUrl = imageUrl,
                    IsPublished = true,
                    PublishedAt = DateTime.UtcNow,
                    AuthorName = "IGRTEC",
                    CreatedAt = DateTime.UtcNow
                };

                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();

                string blogUrl = $"https://igrtec.com/blog/{slug}";
                string socialMsg = $"[GUIA MAESTRA DE INGENIERIA - IGRTEC]\n\nTema: {selectedTopic}\n\nConoce a detalle nuestra perspectiva tecnica en este articulo profundo.\n\nLeer completo: {blogUrl}\n\n#Ingenieria #Servidores #Datacenter #IGRTEC";

                try { await PublishToFacebookOptimized(socialMsg, blogUrl); } catch { }
                try { await PublishLinkedIn(GenerateLinkedInMessage("Ingenieria y Servidores", selectedTopic, blogUrl)); } catch { }

                return Ok(new { success = true, type = "Pillar", title = post.Title });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GeneratePillarContent");
                return StatusCode(500, "Error interno");
            }
        }

        // =========================
        // MOTOR 3: RESOLVEDOR DE ERRORES (SOPORTE TECNICO)
        // =========================

        [HttpGet("generate-error-solver")]
        public async Task<IActionResult> GenerateErrorSolver()
        {
            if (!IsAuthorized()) return Unauthorized(new { error = "Acceso denegado." });

            try
            {
                string[] commonErrors = {
                    "A network-related or instance-specific error occurred while establishing a connection to SQL Server",
                    "504 Gateway Timeout en Nginx",
                    "Fallo de handshake SSL/TLS en servidores IIS"
                };

                string selectedError = commonErrors[new Random().Next(commonErrors.Length)];

                var groqBody = new
                {
                    model = "llama-3.3-70b-versatile",
                    messages = new[] {
                        new { role = "system", content = "Eres Ingeniero Senior en IGRTEC. Resuelve el error con Sintoma, Causa Raiz, Solucion paso a paso y Prevencion. Usa HTML." },
                        new { role = "user", content = $"Soluciona: {selectedError}" }
                    },
                    temperature = 0.3
                };

                var contentReq = new StringContent(JsonSerializer.Serialize(groqBody), Encoding.UTF8, "application/json");
                var gRespReq = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions") { Content = contentReq };
                gRespReq.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _groqApiKey);
                var gResp = await _httpClient.SendAsync(gRespReq);
                var gJson = await gResp.Content.ReadFromJsonAsync<JsonElement>();
                string solutionContent = gJson.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();

                string imageUrl = "https://igrtec.com/images/blog/error-fix.jpg";
                try
                {
                    var pexelsReq = new HttpRequestMessage(HttpMethod.Get, "https://api.pexels.com/v1/search?query=coding+error+server+software&per_page=15");
                    pexelsReq.Headers.Add("Authorization", _pexelsKey);
                    var imgRes = await _httpClient.SendAsync(pexelsReq);
                    if (imgRes.IsSuccessStatusCode)
                    {
                        var iJ = await imgRes.Content.ReadFromJsonAsync<JsonElement>();
                        int totalImages = iJ.GetProperty("photos").GetArrayLength();
                        if (totalImages > 0) imageUrl = iJ.GetProperty("photos")[new Random().Next(0, totalImages)].GetProperty("src").GetProperty("large").GetString();
                    }
                }
                catch { }

                var slug = GenerateSlug($"solucion-{selectedError.Split(' ')[0].ToLower()}");
                if (await _context.BlogPosts.AnyAsync(p => p.Slug == slug)) return Ok("Solucion ya publicada.");

                string cleanTitle = $"Guia: Solucionar {selectedError.Split(' ')[0]}";
                var post = new BlogPost
                {
                    Title = cleanTitle,
                    Slug = slug,
                    Summary = $"Solucion paso a paso para {selectedError.Split(' ')[0]}.",
                    Content = solutionContent,
                    MetaTitle = $"Solucion a {selectedError.Split(' ')[0]} | IGRTEC",
                    MetaDescription = "Guia tecnica paso a paso.",
                    MetaKeywords = "Soporte, IGRTEC, Error IT",
                    FeaturedImageUrl = imageUrl,
                    IsPublished = true,
                    PublishedAt = DateTime.UtcNow,
                    AuthorName = "Soporte IGRTEC",
                    CreatedAt = DateTime.UtcNow
                };

                _context.BlogPosts.Add(post);
                await _context.SaveChangesAsync();

                string blogUrl = $"https://igrtec.com/blog/{slug}";
                string socialMsg = $"[SOPORTE TECNICO - IGRTEC]\n\nTema: {cleanTitle}\n\nConoce la solucion definitiva paso a paso en nuestra documentacion tecnica.\n\nSolucion aqui: {blogUrl}\n\n#SoporteIT #Ingenieria #SysAdmin #IGRTEC";

                try { await PublishToFacebookOptimized(socialMsg, blogUrl); } catch { }
                try { await PublishLinkedIn(GenerateLinkedInMessage("Soporte IT", cleanTitle, blogUrl)); } catch { }

                return Ok(new { success = true, type = "Error Solver" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en GenerateErrorSolver");
                return StatusCode(500, "Error interno");
            }
        }

        // =========================
        // HELPERS DE REDES SOCIALES
        // =========================

        private async Task<string> PublishLinkedIn(string message)
        {
            string token = _config["LinkedIn:AccessToken"];
            // Contingencia: Se publicara temporalmente en el perfil personal (Carlos) hasta que Microsoft apruebe la organizacion IGRTEC.
            string authorUrn = "urn:li:person:pvZDxuUJSb";

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.linkedin.com/v2/ugcPosts");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("LinkedIn-Version", "202602");
            request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

            var body = new Dictionary<string, object> {
                { "author", authorUrn },
                { "lifecycleState", "PUBLISHED" },
                { "specificContent", new Dictionary<string, object> { { "com.linkedin.ugc.ShareContent", new Dictionary<string, object> { { "shareCommentary", new { text = message } }, { "shareMediaCategory", "NONE" } } } } },
                { "visibility", new Dictionary<string, object> { { "com.linkedin.ugc.MemberNetworkVisibility", "PUBLIC" } } }
            };

            request.Content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }

        private async Task<bool> PublishToFacebookOptimized(string message, string linkUrl)
        {
            var pageId = "107013628010918";

            var fbContent = new FormUrlEncodedContent(new[] {
                new KeyValuePair<string,string>("message", message),
                new KeyValuePair<string,string>("link", linkUrl),
                new KeyValuePair<string,string>("access_token", _activeFbPageToken)
            });

            var request = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/v19.0/{pageId}/feed") { Content = fbContent };
            var response = await _httpClient.SendAsync(request);
            if (response.IsSuccessStatusCode) return true;

            try
            {
                // Renovamos los tokens en memoria si caducaron
                var newUserToken = await RenewUserToken(_activeFbUserToken);
                var newPageToken = await GetPageToken(newUserToken);
                _activeFbUserToken = newUserToken;
                _activeFbPageToken = newPageToken;

                // EL DISPARO DE RESCATE
                var retryContent = new FormUrlEncodedContent(new[] {
                    new KeyValuePair<string,string>("message", message),
                    new KeyValuePair<string,string>("link", linkUrl),
                    new KeyValuePair<string,string>("access_token", _activeFbPageToken)
                });
                var retryRequest = new HttpRequestMessage(HttpMethod.Post, $"https://graph.facebook.com/v19.0/{pageId}/feed") { Content = retryContent };
                var retryResponse = await _httpClient.SendAsync(retryRequest);

                return retryResponse.IsSuccessStatusCode;
            }
            catch { return false; }
        }

        private async Task<string> RenewUserToken(string userToken)
        {
            var url = $"https://graph.facebook.com/v19.0/oauth/access_token?grant_type=fb_exchange_token&client_id={_config["Facebook:AppId"]}&client_secret={_config["Facebook:AppSecret"]}&fb_exchange_token={userToken}";
            var json = await _httpClient.GetStringAsync(url);
            return JsonDocument.Parse(json).RootElement.GetProperty("access_token").GetString();
        }

        private async Task<string> GetPageToken(string userToken)
        {
            var url = $"https://graph.facebook.com/v19.0/me/accounts?access_token={userToken}";
            var json = await _httpClient.GetStringAsync(url);
            var data = JsonDocument.Parse(json).RootElement.GetProperty("data");
            return data[0].GetProperty("access_token").GetString();
        }

        // =========================
        // UTILIDADES Y FORMATO
        // =========================

        private string GenerateLinkedInMessage(string keyword, string title, string blogUrl)
        {
            var intro = new[] { "Tendencia detectada", "Nuevo analisis disponible", "Novedad en el sector", "Actualizacion tecnica" };
            return $"{intro[new Random().Next(intro.Length)]}\n\n{title}\n\nAnalisis y articulo completo:\n{blogUrl}\n\n#Tecnologia #Innovacion #IGRTEC";
        }

        private string GenerateSlug(string title) => System.Text.RegularExpressions.Regex.Replace(title.ToLower().Trim(), @"[^a-z0-9\s-]", "").Replace(" ", "-");
    }
}