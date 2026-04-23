using Api_blog.Model;
using Api_bolsatrabajo.Data;
using Api_bolsatrabajo.Model;
using Api_bolsatrabajo.Model.Dtos;
using BolsaDeTrabajo.Api.DTOs;
using BolsaDeTrabajo.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NewsAPI;
using NewsAPI.Constants;
using NewsAPI.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Api_blog.Controllers
{
    [ApiController]
    [Route("api/blog")]
    public class BlogController : ControllerBase
    {
        private readonly BolsatrabajoContext _context;
        string serpApiKey = "503b77863a85bb82a6a694b9d8da6bc7a937c89eb3f824c2150018f1435c9d75";
        string pexeelkey = "MZq2naWjQWysORApzsM57Skid8sEUKaMb40JB97mZVr8qXbvwfj8UZK9";
        private readonly IConfiguration _config;
        private FacebookToken _facebookToken;
        public BlogController(BolsatrabajoContext context, IConfiguration config)
        {
            _context = context;
            _config = config;

            _facebookToken = new FacebookToken
            {
                UserToken = _config["Facebook:UserToken"],
                PageToken = _config["Facebook:PageToken"],
                Expiration = DateTime.UtcNow.AddDays(60)
            };
        }

        // =========================
        // PUBLICO
        // =========================

        // GET: api/blog
        [HttpGet]
        public IActionResult GetAll()
        {
            var posts = Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
     .AsNoTracking(_context.BlogPosts)
     .Where(p => p.IsPublished)
     .OrderByDescending(p => p.PublishedAt)
     .ToList();

            return Ok(posts);
        }

        // GET: api/blog/{slug}
        [HttpGet("{slug}")]
        public IActionResult GetBySlug(string slug)
        {
            if (string.IsNullOrWhiteSpace(slug))
                return BadRequest("Slug inválido");

            var post = _context.BlogPosts
                .FirstOrDefault(p =>
                    p.Slug == slug &&
                    p.IsPublished);

            if (post == null)
                return NotFound();

            // Incrementar vistas
            post.Views++;
            _context.Entry(post).State = Microsoft.EntityFrameworkCore.EntityState.Modified; _context.SaveChanges();

            return Ok(post);
        }


        // =========================
        // ADMIN
        // =========================

        // GET: api/blog/admin
        [HttpGet("admin")]
        public IActionResult GetAdmin()
        {
            var posts = _context.BlogPosts
                .OrderByDescending(p => p.CreatedAt)
                .ToList();

            return Ok(posts);
        }

        // POST: api/blog
        [HttpPost]
        public IActionResult Create([FromBody] BlogPost model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            model.Slug = GenerateSlug(model.Title);
            model.CreatedAt = DateTime.UtcNow;
            model.Views = 0;

            if (model.IsPublished && model.PublishedAt == null)
                model.PublishedAt = DateTime.UtcNow;

            _context.BlogPosts.Add(model);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetBySlug), new { slug = model.Slug }, model);
        }

        // PUT: api/blog/{id}
        [HttpPut("{id}")]
        public IActionResult Update(int id, [FromBody] BlogPost model)
        {
            if (id != model.BlogPostId)
                return BadRequest("Id no coincide");

            var post = _context.BlogPosts.Find(id);
            if (post == null)
                return NotFound();

            post.Title = model.Title;
            post.Summary = model.Summary;
            post.Content = model.Content;
            post.MetaTitle = model.MetaTitle;
            post.MetaDescription = model.MetaDescription;
            post.MetaKeywords = model.MetaKeywords;
            post.FeaturedImageUrl = model.FeaturedImageUrl;
            post.AuthorName = model.AuthorName;
            post.IsPublished = model.IsPublished;
            post.PublishedAt = model.IsPublished
                ? model.PublishedAt ?? DateTime.UtcNow
                : null;
            post.UpdatedAt = DateTime.UtcNow;
            post.Slug = GenerateSlug(model.Title);

            _context.Update(post);  
            _context.SaveChanges();

            return Ok(post);
        }

        // DELETE: api/blog/{id}
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var post = _context.BlogPosts.Find(id);
            if (post == null)
                return NotFound();

            _context.BlogPosts.Remove(post);
            _context.SaveChanges();

            return NoContent();
        }



        private string NormalizeKeyword(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return "";

            keyword = keyword.ToLowerInvariant();

            // quitar acentos
            keyword = keyword
                .Replace("á", "a")
                .Replace("é", "e")
                .Replace("í", "i")
                .Replace("ó", "o")
                .Replace("ú", "u")
                .Replace("ñ", "n");

            // eliminar caracteres especiales
            keyword = System.Text.RegularExpressions.Regex
                .Replace(keyword, @"[^a-z0-9\s]", " ");

            // eliminar números
            keyword = System.Text.RegularExpressions.Regex
                .Replace(keyword, @"\d+", "");

            var stopWords = new HashSet<string>
    {
        // español comunes
        "nuevo","nueva","nuevos","nuevas",
        "que","es","como","cuando","donde","porque",
        "precio","cuanto","vale",
        "analisis","opinion","review",
        "oficial","lanzamiento",
        "version","modelo","serie",
        "mexico","mx","usa","latam",

        // ingles comunes
        "latest","new","what","is","how","why",
        "price","release","launch",
        "official","specs","review",

        // comparaciones
        "vs","versus","contra","comparison",

        // conectores
        "de","del","la","el","los","las","en","para","por","con","y","o",

        // años comunes
        "2023","2024","2025","2026","2027"
    };

            var words = keyword
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Where(w => !stopWords.Contains(w));

            keyword = string.Join(" ", words);

            // quitar espacios duplicados
            keyword = System.Text.RegularExpressions.Regex
                .Replace(keyword, @"\s+", " ");

            return keyword.Trim();
        }

        private bool TokenExpiringSoon(DateTime expiration)
        {
            return DateTime.UtcNow.AddDays(5) >= expiration;
        }
        private async Task<string> RenewUserToken(string userToken)
        {
            var url =
                $"https://graph.facebook.com/v19.0/oauth/access_token" +
                $"?grant_type=fb_exchange_token" +
                $"&client_id={_config["Facebook:AppId"]}" +
                $"&client_secret={_config["Facebook:AppSecret"]}" +
                $"&fb_exchange_token={userToken}";

            using var http = new HttpClient();

            var response = await http.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);


            return doc.RootElement
                .GetProperty("access_token")
                .GetString();
        }
        private async Task<string> GetPageToken(string userToken)
        {
            using var http = new HttpClient();

            var url =
                $"https://graph.facebook.com/v19.0/me/accounts?access_token={userToken}";

            var response = await http.GetAsync(url);

            var json = await response.Content.ReadAsStringAsync();

            var doc = JsonDocument.Parse(json);

            var data = doc.RootElement.GetProperty("data");

            if (data.GetArrayLength() == 0)
                return null;

            return data[0]
                .GetProperty("access_token")
                .GetString();
        }


        [HttpGet("test-facebook")]
        public async Task<IActionResult> TestFacebookPost()
        {
            try
            {
                var pageId = "107013628010918";
                var token = _facebookToken?.PageToken;

                if (string.IsNullOrWhiteSpace(token))
                {
                    return BadRequest(new
                    {
                        success = false,
                        step = "validacion_token",
                        message = "PageToken está vacío o null"
                    });
                }

                var message = "🚀 Prueba automática desde API IGRTEC";
                var link = "https://igrtec.com";

                using var http = new HttpClient();

                var content = new FormUrlEncodedContent(new[]
                {
            new KeyValuePair<string,string>("message", message),
            new KeyValuePair<string,string>("link", link),
            new KeyValuePair<string,string>("access_token", token)
        });

                var response = await http.PostAsync(
                    $"https://graph.facebook.com/v19.0/{pageId}/feed",
                    content);

                var result = await response.Content.ReadAsStringAsync();

                // Si publicó bien
                if (response.IsSuccessStatusCode)
                {
                    return Ok(new
                    {
                        success = true,
                        refreshedToken = false,
                        step = "post_original",
                        facebookResponse = result
                    });
                }

                // Si falló, intentar renovar
                try
                {
                    var newUserToken = await RenewUserToken(_facebookToken.UserToken);

                    if (string.IsNullOrWhiteSpace(newUserToken))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            step = "renew_user_token",
                            message = "RenewUserToken devolvió null o vacío",
                            facebookResponse = result
                        });
                    }

                    var newPageToken = await GetPageToken(newUserToken);

                    if (string.IsNullOrWhiteSpace(newPageToken))
                    {
                        return BadRequest(new
                        {
                            success = false,
                            step = "get_page_token",
                            message = "GetPageToken devolvió null o vacío",
                            facebookResponse = result
                        });
                    }

                    _facebookToken.UserToken = newUserToken;
                    _facebookToken.PageToken = newPageToken;

                    var retryContent = new FormUrlEncodedContent(new[]
                    {
                new KeyValuePair<string,string>("message", message),
                new KeyValuePair<string,string>("link", link),
                new KeyValuePair<string,string>("access_token", newPageToken)
            });

                    var retry = await http.PostAsync(
                        $"https://graph.facebook.com/v19.0/{pageId}/feed",
                        retryContent);

                    var retryResult = await retry.Content.ReadAsStringAsync();

                    return Ok(new
                    {
                        success = retry.IsSuccessStatusCode,
                        refreshedToken = true,
                        step = "post_retry",
                        firstFacebookResponse = result,
                        retryFacebookResponse = retryResult
                    });
                }
                catch (Exception renewEx)
                {
                    return BadRequest(new
                    {
                        success = false,
                        step = "renew_or_retry",
                        message = renewEx.Message,
                        facebookResponse = result
                    });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    step = "general",
                    message = ex.Message,
                    inner = ex.InnerException?.Message
                });
            }
        }
        [HttpPost("auto-trend")]
        public async Task<IActionResult> AutoTrendPost()
        {
            var today = DateTime.UtcNow.Date;
            int maxPostsPerDay = 3;

            var postsToday = _context.BlogPosts
                .Count(p => p.CreatedAt.Date == today);

            if (postsToday >= maxPostsPerDay)
            {
                return Ok(new
                {
                    success = false,
                    message = "Límite diario alcanzado"
                });
            }

            using var http = new HttpClient();

       

            // =========================
            // CONSULTAR TRENDS
            // =========================

            var url =
                $"https://serpapi.com/search.json?engine=google_trends_trending_now&geo=MX&api_key={serpApiKey}";

            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return BadRequest("Error consultando tendencias");

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var trends = doc.RootElement.GetProperty("trending_searches");

            string keyword = null;

            // =========================
            // FILTRO ORIGINAL (EL QUE TE FUNCIONABA)
            // =========================
            // =========================
            // FILTROS DE RELEVANCIA
            // =========================

            var excludedKeywords = new[]
            {
    "infinitum","telmex","starlink",
    "whatsapp","facebook","instagram","tiktok",
    "netflix","spotify",
    "iphone","samsung",
    "miami","united","liga","futbol","soccer","nba"
};

            var businessTechKeywords = new[]
            {
    "software","erp","crm","api","cloud","nube",
    "inteligencia artificial","ai",
    "machine learning","big data",
    "ciberseguridad","seguridad",
    "iot","internet de las cosas",
    "automatizacion","robotica",
    "datacenter","servidor",
    "nvidia","amd","intel",
    "programacion","developer",
    "sistema","digital","tecnologia"
};

            foreach (var t in trends.EnumerateArray())
            {
                var k = t.GetProperty("query").GetString();

                if (string.IsNullOrWhiteSpace(k))
                    continue;

                var keywordLower = k.ToLower();

                // excluir keywords no deseadas
                if (excludedKeywords.Any(e => keywordLower.Contains(e)))
                    continue;

                // verificar categoria technology
                if (t.TryGetProperty("categories", out var categories) &&
                    categories.GetArrayLength() > 0)
                {
                    var categoryName = categories[0].GetProperty("name").GetString();

                    if (categoryName != "Technology")
                        continue;
                }
                else
                {
                    continue;
                }

                // verificar que tenga relación con tecnología empresarial
                bool isBusinessTech = businessTechKeywords
                    .Any(tk => keywordLower.Contains(tk));

                if (!isBusinessTech)
                    continue;

                var normalized = NormalizeKeyword(k);

                var existsKeyword = _context.BlogPosts
                    .AsEnumerable()
                    .Any(p => NormalizeKeyword(p.MetaKeywords) == normalized);

                if (!existsKeyword)
                {
                    keyword = k;
                    break;
                }
            }

            if (keyword == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "No se encontró tendencia nueva"
                });
            }

            // =========================
            // TITULO
            // =========================

            string title = $"¿Qué está pasando con {keyword}? Tendencia tecnológica en México";

            var slug = GenerateSlug(title);

            if (_context.BlogPosts.Any(p => p.Slug == slug))
            {
                return Ok(new
                {
                    success = false,
                    message = "Post duplicado"
                });
            }

            // =========================
            // GOOGLE NEWS
            // =========================

            string newsContent = "";

            try
            {
                var newsUrl =
                    $"https://serpapi.com/search.json?engine=google_news&q={keyword}&hl=es&gl=mx&api_key={serpApiKey}";

                var newsResponse = await http.GetAsync(newsUrl);

                if (newsResponse.IsSuccessStatusCode)
                {
                    var newsJson = await newsResponse.Content.ReadAsStringAsync();

                    using var newsDoc = JsonDocument.Parse(newsJson);

                    if (newsDoc.RootElement.TryGetProperty("news_results", out var news))
                    {
                        int count = Math.Min(3, news.GetArrayLength());

                        for (int i = 0; i < count; i++)
                        {
                            var titleNews = news[i].GetProperty("title").GetString();
                            var snippet = news[i].GetProperty("snippet").GetString();

                            newsContent += $"<p><strong>{titleNews}</strong>: {snippet}</p>";
                        }
                    }
                }
            }
            catch { }

            // =========================
            // RELATED SEARCHES
            // =========================

            string relatedList = "";

            try
            {
                var searchUrl =
                    $"https://serpapi.com/search.json?engine=google&q={keyword}&hl=es&gl=mx&api_key={serpApiKey}";

                var searchResponse = await http.GetAsync(searchUrl);

                if (searchResponse.IsSuccessStatusCode)
                {
                    var searchJson = await searchResponse.Content.ReadAsStringAsync();

                    using var searchDoc = JsonDocument.Parse(searchJson);

                    if (searchDoc.RootElement.TryGetProperty("related_searches", out var related))
                    {
                        int count = Math.Min(5, related.GetArrayLength());

                        relatedList += "<ul>";

                        for (int i = 0; i < count; i++)
                        {
                            var query = related[i].GetProperty("query").GetString();

                            relatedList += $"<li>{query}</li>";
                        }

                        relatedList += "</ul>";
                    }
                }
            }
            catch { }

            // =========================
            // IMAGEN
            // =========================

            string imageUrl = null;

            try
            {
                http.DefaultRequestHeaders.Add("Authorization", pexeelkey);

                var imgResponse =
                    await http.GetAsync($"https://api.pexels.com/v1/search?query={keyword}&per_page=1");

                if (imgResponse.IsSuccessStatusCode)
                {
                    var imgJson = await imgResponse.Content.ReadAsStringAsync();

                    using var imgDoc = JsonDocument.Parse(imgJson);

                    var photos = imgDoc.RootElement.GetProperty("photos");

                    if (photos.GetArrayLength() > 0)
                    {
                        imageUrl = photos[0]
                            .GetProperty("src")
                            .GetProperty("large")
                            .GetString();
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = "/images/blog/technology.jpg";

            // =========================
            // CONTENIDO MEJORADO
            // =========================

            string intro =
                $"En las últimas horas, <strong>{keyword}</strong> se ha convertido en una de las búsquedas tecnológicas más populares en México. Cuando un tema comienza a aparecer entre las tendencias de Google, normalmente refleja un cambio importante dentro del ecosistema digital o el lanzamiento de nuevas tecnologías.";

            string analysis =
                $"El crecimiento de interés alrededor de <strong>{keyword}</strong> demuestra cómo ciertas innovaciones o eventos tecnológicos pueden captar rápidamente la atención de usuarios, empresas y desarrolladores.";

            string impact =
                $"Para empresas tecnológicas y profesionales del sector digital, seguir tendencias como <strong>{keyword}</strong> permite identificar oportunidades de innovación, desarrollo de software y adopción de nuevas herramientas.";

            string future =
                $"Si la tendencia continúa creciendo, es posible que <strong>{keyword}</strong> se convierta en uno de los temas más discutidos dentro del sector tecnológico durante los próximos días.";

            string content =
        $@"
<h2>{title}</h2>

<p>{intro}</p>

<h3>Noticias recientes sobre {keyword}</h3>
{newsContent}

<h3>Análisis de la tendencia</h3>
<p>{analysis}</p>

<h3>Impacto en la industria tecnológica</h3>
<p>{impact}</p>

<h3>Búsquedas relacionadas</h3>
{relatedList}

<h3>¿Qué podemos esperar?</h3>
<p>{future}</p>
";

            // =========================
            // CREAR POST
            // =========================

            var post = new BlogPost
            {
                Title = title,
                Summary = $"Análisis sobre la tendencia tecnológica {keyword}",
                Content = content,
                MetaTitle = title,
                MetaDescription = $"Todo sobre {keyword} y su impacto en tecnología.",
                MetaKeywords = keyword,
                AuthorName = "IGRTEC",
                Slug = slug,
                FeaturedImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true,
                Views = 0
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                keyword = keyword,
                slug = slug,
                image = imageUrl
            });
        }
        [HttpGet("auto-trend23")]
        public async Task<IActionResult> AutoTrendPos2t()
        {
            var today = DateTime.UtcNow.Date;
            int maxPostsPerDay = 3;

            // =========================
            // VALIDAR POSTS DEL DIA
            // =========================

            var postsToday = _context.BlogPosts
                .Count(p => p.CreatedAt.Date == today);

            if (postsToday >= maxPostsPerDay)
            {
                return Ok(new
                {
                    success = false,
                    message = "Límite diario alcanzado"
                });
            }

            using var http = new HttpClient();

            // =========================
            // CONSULTAR TRENDS
            // =========================

            string serpApiKey = "503b77863a85bb82a6a694b9d8da6bc7a937c89eb3f824c2150018f1435c9d75";

            var url =
                $"https://serpapi.com/search.json?engine=google_trends_trending_now&geo=MX&api_key={serpApiKey}";

            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return BadRequest("Error consultando tendencias");

            var json = await response.Content.ReadAsStringAsync();

            using var doc = System.Text.Json.JsonDocument.Parse(json);

            var trends = doc.RootElement.GetProperty("trending_searches");

            string keyword = null;

            // =========================
            // BUSCAR TREND VALIDO
            // =========================

            foreach (var t in trends.EnumerateArray())
            {
                var k = t.GetProperty("query").GetString();

                // verificar categoria
                if (t.TryGetProperty("categories", out var categories) &&
                    categories.GetArrayLength() > 0)
                {
                    var categoryName = categories[0].GetProperty("name").GetString();

                    if (categoryName != "Technology")
                        continue; // ignorar si no es tecnologia
                }
                else
                {
                    continue;
                }

                var normalized = NormalizeKeyword(k);
                var existsKeyword = _context.BlogPosts
                    .AsEnumerable()
                    .Any(p => NormalizeKeyword(p.MetaKeywords) == normalized);

                if (!existsKeyword)
                {
                    keyword = k;
                    break;
                }
            }

            if (keyword == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "No se encontró tendencia nueva"
                });
            }

            // =========================
            // GENERAR TITULO
            // =========================

            string title = $"¿Qué está pasando con {keyword}? Tendencia tecnológica en México";

            var slug = GenerateSlug(title);

            // =========================
            // VALIDAR DUPLICADOS
            // =========================

            var exists = _context.BlogPosts
                .Any(p => p.Slug == slug);

            if (exists)
            {
                return Ok(new
                {
                    success = false,
                    message = "Post duplicado"
                });
            }

            // =========================
            // BUSCAR IMAGEN PEXELS
            // =========================

            string imageUrl = null;

            try
            {
                http.DefaultRequestHeaders.Add("Authorization", "MZq2naWjQWysORApzsM57Skid8sEUKaMb40JB97mZVr8qXbvwfj8UZK9");

                var imgResponse = await http.GetAsync(
                    $"https://api.pexels.com/v1/search?query={keyword}&per_page=1");

                if (imgResponse.IsSuccessStatusCode)
                {
                    var imgJson = await imgResponse.Content.ReadAsStringAsync();

                    using var imgDoc = System.Text.Json.JsonDocument.Parse(imgJson);

                    var photos = imgDoc.RootElement.GetProperty("photos");

                    if (photos.GetArrayLength() > 0)
                    {
                        imageUrl = photos[0]
                            .GetProperty("src")
                            .GetProperty("large")
                            .GetString();
                    }
                }
            }
            catch { }

            // fallback si no hay imagen
            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = "/images/blog/technology.jpg";

            // =========================
            // CREAR CONTENIDO
            // =========================

            var introTemplates = new[]
            {
    $"En las últimas horas, el término <strong>{keyword}</strong> ha comenzado a aparecer entre las búsquedas más populares en México, lo que indica que el tema está captando la atención de miles de usuarios.",

    $"El interés por <strong>{keyword}</strong> ha crecido rápidamente en internet durante las últimas horas. Este tipo de tendencias suele reflejar nuevos lanzamientos, anuncios o cambios dentro del sector tecnológico.",

    $"Las búsquedas relacionadas con <strong>{keyword}</strong> están aumentando en México. Cuando un tema comienza a posicionarse entre las tendencias, normalmente significa que algo relevante está ocurriendo en el ecosistema digital.",

    $"Durante las últimas horas, <strong>{keyword}</strong> ha comenzado a generar conversación entre usuarios y medios tecnológicos. Este incremento en las búsquedas suele estar relacionado con novedades o anuncios recientes.",

    $"El término <strong>{keyword}</strong> se ha posicionado entre las búsquedas destacadas del momento. Este tipo de movimientos suele indicar que el tema está despertando interés dentro del mundo tecnológico.",

    $"En el entorno digital actual, cuando un término como <strong>{keyword}</strong> comienza a destacar en las tendencias de búsqueda, suele ser una señal de que algo importante está ocurriendo alrededor de esa tecnología o producto."
};

            var impactTemplates = new[]
            {
    $"Para empresas y profesionales del sector tecnológico, este tipo de tendencias puede ofrecer pistas sobre hacia dónde se está moviendo el mercado digital.",

    $"Observar este tipo de tendencias permite entender qué tecnologías están despertando mayor interés entre los usuarios y las empresas.",

    $"El crecimiento de estas búsquedas suele reflejar cambios en el comportamiento digital de los usuarios o el lanzamiento de nuevas soluciones tecnológicas.",

    $"Cuando un tema comienza a ganar relevancia en las búsquedas, muchas veces está relacionado con innovaciones, actualizaciones o nuevas herramientas que comienzan a captar la atención del público.",

    $"Analizar estas tendencias también ayuda a identificar oportunidades tecnológicas que podrían impactar a empresas, desarrolladores y usuarios en los próximos meses."
};
            var closingTemplates = new[]
            {
    $"Habrá que observar cómo evoluciona el interés por <strong>{keyword}</strong> durante los próximos días y si continúa posicionándose entre los temas más buscados.",

    $"Seguiremos atentos a las novedades relacionadas con <strong>{keyword}</strong> y al impacto que pueda tener dentro del ecosistema tecnológico.",

    $"El comportamiento de estas tendencias puede ofrecer señales tempranas sobre cambios en el sector digital y las tecnologías que podrían dominar las próximas conversaciones.",

    $"En un entorno tecnológico que evoluciona rápidamente, este tipo de tendencias suele anticipar novedades relevantes para empresas y usuarios.",

    $"A medida que el interés por <strong>{keyword}</strong> continúa creciendo, será interesante ver qué desarrollos o anuncios aparecen alrededor de este tema."
};


            var contextTemplates = new[]
{
    $"En muchos casos, cuando un término como <strong>{keyword}</strong> comienza a generar búsquedas masivas, suele estar vinculado a anuncios recientes, actualizaciones de producto o nuevas funcionalidades.",

    $"Este tipo de picos en las búsquedas suele aparecer cuando una tecnología empieza a captar atención en redes sociales, medios especializados o eventos del sector.",

    $"La rapidez con la que <strong>{keyword}</strong> ha comenzado a aparecer en las búsquedas refleja cómo la información tecnológica puede difundirse rápidamente en el entorno digital."
};
            var random = new Random();

            string intro = introTemplates[random.Next(introTemplates.Length)];
            string impact = impactTemplates[random.Next(impactTemplates.Length)];
            string closing = closingTemplates[random.Next(closingTemplates.Length)];
            string context = contextTemplates[random.Next(contextTemplates.Length)];
            string content =
                $"<h2>{keyword}</h2>" +
                $"<p>{intro}</p>" +
                $"<p>{impact}</p>" +
                $"<p>{closing}</p>";

            // =========================
            // CREAR POST
            // =========================

            var post = new BlogPost
            {
                Title = title,
                Summary = $"Análisis sobre la tendencia {keyword}",
                Content = content,
                MetaTitle = title,
                MetaDescription = $"Todo sobre {keyword} y su impacto en tecnología.",
                MetaKeywords = keyword,
                AuthorName = "IGRTEC",
                Slug = slug,
                FeaturedImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true,
                Views = 0
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();


            // =========================
            // PUBLICAR EN FACEBOOK
            // =========================

            string blogUrl = $"https://igrtec.com/blog/{slug}";

            string facebookMessage =
            $@"🚀 Nueva tendencia tecnológica detectada

{title}

{keyword} está creciendo rápidamente en las búsquedas de tecnología en México.

Lee el análisis completo en nuestro blog 👇
{blogUrl}

#Tecnologia #Software #IGRTEC #Innovacion";

            var pageId = "107013628010918";
            var token = _facebookToken.PageToken;

            var fbContent = new FormUrlEncodedContent(new[]
            {
    new KeyValuePair<string,string>("message", facebookMessage),
    new KeyValuePair<string,string>("link", blogUrl),
    new KeyValuePair<string,string>("access_token", token)
});

            var fbResponse = await http.PostAsync(
                $"https://graph.facebook.com/v19.0/{pageId}/feed",
                fbContent);

            var fbResult = await fbResponse.Content.ReadAsStringAsync();


            // SI FALLA → RENOVAR TOKEN Y REINTENTAR
            if (!fbResponse.IsSuccessStatusCode)
            {
                var newUserToken = await RenewUserToken(_facebookToken.UserToken);
                var newPageToken = await GetPageToken(newUserToken);

                _facebookToken.UserToken = newUserToken;
                _facebookToken.PageToken = newPageToken;

                var retryContent = new FormUrlEncodedContent(new[]
                {
        new KeyValuePair<string,string>("message", facebookMessage),
        new KeyValuePair<string,string>("link", blogUrl),
        new KeyValuePair<string,string>("access_token", newPageToken)
    });

                await http.PostAsync(
                    $"https://graph.facebook.com/v19.0/{pageId}/feed",
                    retryContent);
            }


            // generar mensaje dinámico
            string linkedinMessage = GenerateLinkedInMessage(facebookMessage, title, blogUrl);

            try
            {
                //var result = await PublishLinkedIn(linkedinMessage);

                return Ok(new
                {
                    success = true,
                    message = linkedinMessage,
                    linkedinResponse = ""
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
            return Ok(new
            {
                success = true,
                keyword = keyword,
                slug = slug,
                image = imageUrl
            });
        }
        [HttpGet("auto-trend2")]
        public async Task<IActionResult> AutoTrendPos2t3()
        {
            var today = DateTime.UtcNow.Date;
            int maxPostsPerDay = 3;

            var postsToday = _context.BlogPosts
                .Count(p => p.CreatedAt.Date == today);

            if (postsToday >= maxPostsPerDay)
            {
                return Ok(new
                {
                    success = false,
                    message = "Límite diario alcanzado"
                });
            }

            using var http = new HttpClient();

            string serpApiKey = "503b77863a85bb82a6a694b9d8da6bc7a937c89eb3f824c2150018f1435c9d75";

            // =========================
            // CONSULTAR TRENDS
            // =========================

            var url =
                $"https://serpapi.com/search.json?engine=google_trends_trending_now&geo=MX&api_key={serpApiKey}";

            var response = await http.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return BadRequest("Error consultando tendencias");

            var json = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);

            var trends = doc.RootElement.GetProperty("trending_searches");

            string keyword = null;

            foreach (var t in trends.EnumerateArray())
            {
                var k = t.GetProperty("query").GetString();

                if (t.TryGetProperty("categories", out var categories) &&
                    categories.GetArrayLength() > 0)
                {
                    var categoryName = categories[0].GetProperty("name").GetString();

                    if (categoryName != "Technology")
                        continue;
                }
                else
                {
                    continue;
                }

                var normalized = NormalizeKeyword(k);

                var existsKeyword = _context.BlogPosts
                    .AsEnumerable()
                    .Any(p => NormalizeKeyword(p.MetaKeywords) == normalized);

                if (!existsKeyword)
                {
                    keyword = k;
                    break;
                }
            }

            if (keyword == null)
            {
                return Ok(new
                {
                    success = false,
                    message = "No se encontró tendencia nueva"
                });
            }

            // =========================
            // BUSCAR NOTICIA REAL
            // =========================

            string newsTitle = "";
            string newsSnippet = "";
            string newsSource = "";
            string newsLink = "";

            try
            {
                var newsUrl =
                    $"https://serpapi.com/search.json?q={keyword}&tbm=nws&hl=es&gl=mx&api_key={serpApiKey}";

                var newsResponse = await http.GetAsync(newsUrl);

                if (newsResponse.IsSuccessStatusCode)
                {
                    var newsJson = await newsResponse.Content.ReadAsStringAsync();

                    using var newsDoc = JsonDocument.Parse(newsJson);

                    if (newsDoc.RootElement.TryGetProperty("news_results", out var news))
                    {
                        if (news.GetArrayLength() > 0)
                        {
                            var article = news[0];

                            newsTitle = article.GetProperty("title").GetString();
                            newsSnippet = article.GetProperty("snippet").GetString();
                            newsSource = article.GetProperty("source").GetString();
                            newsLink = article.GetProperty("link").GetString();
                        }
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(newsTitle))
            {
                newsTitle = keyword;
            }

            // =========================
            // GENERAR TITULO
            // =========================

            string title = newsTitle;

            var slug = GenerateSlug(title);

            var exists = _context.BlogPosts
                .Any(p => p.Slug == slug);

            if (exists)
            {
                return Ok(new
                {
                    success = false,
                    message = "Post duplicado"
                });
            }

            // =========================
            // BUSCAR IMAGEN
            // =========================

            string imageUrl = null;

            try
            {
                http.DefaultRequestHeaders.Add("Authorization", "MZq2naWjQWysORApzsM57Skid8sEUKaMb40JB97mZVr8qXbvwfj8UZK9");

                var imgResponse = await http.GetAsync(
                    $"https://api.pexels.com/v1/search?query={keyword}&per_page=1");

                if (imgResponse.IsSuccessStatusCode)
                {
                    var imgJson = await imgResponse.Content.ReadAsStringAsync();

                    using var imgDoc = JsonDocument.Parse(imgJson);

                    var photos = imgDoc.RootElement.GetProperty("photos");

                    if (photos.GetArrayLength() > 0)
                    {
                        imageUrl = photos[0]
                            .GetProperty("src")
                            .GetProperty("large")
                            .GetString();
                    }
                }
            }
            catch { }

            if (string.IsNullOrEmpty(imageUrl))
                imageUrl = "/images/blog/technology.jpg";

            // =========================
            // CONTENIDO BASADO EN NOTICIA REAL
            // =========================

            string content = $@"

<h1>{newsTitle}</h1>

<p><strong>Fuente:</strong> {newsSource}</p>

<p>
{newsSnippet}
</p>

<h2>¿Por qué esta noticia es relevante?</h2>

<p>
La noticia anterior ha generado un incremento significativo en las búsquedas relacionadas con 
<strong>{keyword}</strong>. Cuando un tema comienza a aparecer en medios especializados 
y portales tecnológicos, es común que miles de usuarios busquen más información sobre el tema.
</p>

<h2>Impacto en el sector tecnológico</h2>

<p>
Las tendencias tecnológicas suelen reflejar cambios importantes dentro de la industria digital.
Empresas, desarrolladores y organizaciones analizan estas noticias para entender hacia dónde 
se dirige la innovación y qué tecnologías podrían dominar el mercado en los próximos meses.
</p>

<h2>Leer noticia original</h2>

<p>
<a href='{newsLink}' target='_blank'>
Ver artículo completo en {newsSource}
</a>
</p>

<hr>

<p>
<em>
Este artículo forma parte de las  tendencias tecnológicas 
realizado por IGRTEC para identificar innovaciones relevantes dentro del 
ecosistema digital.
</em>
</p>
";

            // =========================
            // CREAR POST
            // =========================

            var post = new BlogPost
            {
                Title = title,
                Summary = newsSnippet,
                Content = content,
                MetaTitle = title,
                MetaDescription = newsSnippet,
                MetaKeywords = keyword,
                AuthorName = "IGRTEC",
                Slug = slug,
                FeaturedImageUrl = imageUrl,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                PublishedAt = DateTime.UtcNow,
                IsPublished = true,
                Views = 0
            };

            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();

            // =========================
            // FACEBOOK POST
            // =========================

            string blogUrl = $"https://igrtec.com/blog/{slug}";

            string facebookMessage =
        $@"🚀 {newsTitle}

{newsSnippet}

Lee el análisis completo 👇
{blogUrl}

#Tecnologia #Software #IGRTEC #Innovacion";

            var pageId = "107013628010918";
            var token = _facebookToken.PageToken;

            var fbContent = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string,string>("message", facebookMessage),
        new KeyValuePair<string,string>("link", blogUrl),
        new KeyValuePair<string,string>("access_token", token)
    });

            await http.PostAsync(
                $"https://graph.facebook.com/v19.0/{pageId}/feed",
                fbContent);

            return Ok(new
            {
                success = true,
                keyword,
                slug,
                imageUrl
            });
        }


[HttpGet("auto-news-tech")]
    public async Task<IActionResult> AutoNewsTech()
    {
        var today = DateTime.UtcNow.Date;
        int maxPostsPerDay = 5;

        var postsToday = _context.BlogPosts
            .Count(p => p.CreatedAt.Date == today);

        if (postsToday >= maxPostsPerDay)
        {
            return Ok(new
            {
                success = false,
                message = "Límite diario alcanzado"
            });
        }

        // =========================
        // CONSULTAR NEWS API
        // =========================

        var newsApiClient = new NewsApiClient("c0a6855dab554242b3db0aaf72e5f632");

        var articlesResponse = newsApiClient.GetEverything(new EverythingRequest
        {
            Q = "artificial intelligence OR AI OR technology OR software",
            SortBy = SortBys.PublishedAt,
            Language = Languages.ES,
            PageSize = 20
        });

        if (articlesResponse.Status != Statuses.Ok)
            return BadRequest("Error consultando NewsAPI");

        Article selectedArticle = null;
            var techKeywords = new[]
            {
    "ai","artificial","inteligencia","technology","tecnologia",
    "software","startup","robot","machine learning","openai",
    "google","microsoft","apple","nvidia","meta","chatgpt"
};

            foreach (var article in articlesResponse.Articles)
            {
                var text = (article.Title + " " + article.Description).ToLower();

                bool isTech = techKeywords.Any(k => text.Contains(k));

                if (!isTech)
                    continue;

                var slugTest = GenerateSlug(article.Title);

                var exists = _context.BlogPosts.Any(p => p.Slug == slugTest);

                if (!exists)
                {
                    selectedArticle = article;
                    break;
                }
            }

            if (selectedArticle == null)
        {
            return Ok(new
            {
                success = false,
                message = "No se encontró noticia nueva"
            });
        }

        string title = selectedArticle.Title;
        string description = selectedArticle.Description;
        string source = selectedArticle.Source.Name;
        string url = selectedArticle.Url;

        var slug = GenerateSlug(title);

        // =========================
        // BUSCAR IMAGEN PEXELS
        // =========================

        using var http = new HttpClient();

        string imageUrl = null;
            var query = Uri.EscapeDataString(title);

            try
            {
                http.DefaultRequestHeaders.Add("Authorization", "MZq2naWjQWysORApzsM57Skid8sEUKaMb40JB97mZVr8qXbvwfj8UZK9");

                var imgResponse = await http.GetAsync(
                $"https://api.pexels.com/v1/search?query={query}&per_page=5");

            if (imgResponse.IsSuccessStatusCode)
            {
                var imgJson = await imgResponse.Content.ReadAsStringAsync();

                using var imgDoc = JsonDocument.Parse(imgJson);

                var photos = imgDoc.RootElement.GetProperty("photos");

                if (photos.GetArrayLength() > 0)
                {
                        var index = Random.Shared.Next(photos.GetArrayLength());

                        var photo = photos[index];

                        imageUrl = photo
                            .GetProperty("src")
                            .GetProperty("large")
                            .GetString();
                    }
            }
        }
        catch { }

        if (string.IsNullOrEmpty(imageUrl))
            imageUrl = "/images/blog/technology.jpg";

        // =========================
        // CONTENIDO DEL POST
        // =========================

        string content = $@"

<h1>{title}</h1>

<p><strong>Fuente:</strong> {source}</p>

<p>{description}</p>

<h2>Contexto tecnológico</h2>

<p>
La noticia anterior refleja uno de los avances recientes dentro del
ecosistema tecnológico. La inteligencia artificial y el desarrollo
de software continúan evolucionando rápidamente, generando nuevas
oportunidades para empresas, startups y desarrolladores.
</p>

<h2>Impacto en la industria</h2>

<p>
Este tipo de noticias demuestra cómo la innovación tecnológica está
transformando múltiples sectores. Empresas tecnológicas utilizan
estos avances para crear nuevas plataformas, herramientas digitales
y soluciones basadas en inteligencia artificial.
</p>

<h2>Leer noticia original</h2>

<p>
<a href='{url}' target='_blank'>
Ver artículo completo en {source}
</a>
</p>

<hr>

<p>
<em>
Artículo generado automáticamente por el sistema de monitoreo
tecnológico de IGRTEC.
</em>
</p>
";

        // =========================
        // CREAR POST
        // =========================

        var post = new BlogPost
        {
            Title = title,
            Summary = description,
            Content = content,
            MetaTitle = title,
            MetaDescription = description,
            MetaKeywords = "AI, Artificial Intelligence, Technology",
            AuthorName = "IGRTEC",
            Slug = slug,
            FeaturedImageUrl = imageUrl,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            PublishedAt = DateTime.UtcNow,
            IsPublished = true,
            Views = 0
        };

        _context.BlogPosts.Add(post);
        await _context.SaveChangesAsync();
            // =========================
            // PUBLICAR EN FACEBOOK
            // =========================

            string blogUrl = $"https://igrtec.com/blog/{slug}";

            // generar mensaje más natural
            string facebookMessage =
  $@"📰 Noticias de tecnología

{title}

{description}

📰 Fuente: {source}

Lee el análisis completo en nuestro blog:
{blogUrl}

#InteligenciaArtificial #Tecnologia #Software #Innovacion #IGRTEC";

            var pageId = "107013628010918";
            var token = _facebookToken.PageToken;

            var fbContent = new FormUrlEncodedContent(new[]
            {
    new KeyValuePair<string,string>("message", facebookMessage),
    new KeyValuePair<string,string>("link", blogUrl),
    new KeyValuePair<string,string>("access_token", token)
});
                
            var fbResponse = await http.PostAsync(
                $"https://graph.facebook.com/v19.0/{pageId}/feed",
                fbContent);

            var fbResult = await fbResponse.Content.ReadAsStringAsync();

            return Ok(new
        {
            success = true,
            title,
            slug,
            imageUrl
        });
    }
    private string GenerateLinkedInMessage(string keyword, string title, string blogUrl)
        {
            var intro = new[]
            {
        $"🚀 Tendencia tecnológica detectada",
        $"📈 {keyword} comienza a generar conversación en el sector tecnológico",
        $"🔎 Nueva tendencia digital en crecimiento",
        $"💡 {keyword} está captando atención en el mundo tecnológico",
        $"🌐 Tecnología en tendencia: {keyword}"
    };

            var context = new[]
            {
        $"Durante las últimas horas, \"{keyword}\" ha comenzado a aparecer entre las búsquedas tecnológicas más populares en México.",
        $"El interés por \"{keyword}\" ha aumentado rápidamente en las búsquedas tecnológicas.",
        $"Cuando un término como \"{keyword}\" empieza a posicionarse en tendencias de búsqueda, suele ser señal de que algo relevante está ocurriendo.",
        $"Las tendencias tecnológicas muchas veces reflejan innovaciones o cambios relevantes dentro del ecosistema digital.",
        $"El crecimiento de búsquedas sobre \"{keyword}\" sugiere que el tema está despertando interés dentro del sector tecnológico."
    };

            var cta = new[]
            {
        $"En nuestro blog analizamos qué está pasando con esta tendencia.",
        $"Publicamos un breve análisis sobre el contexto de esta tendencia tecnológica.",
        $"Revisamos qué está ocurriendo alrededor de este tema.",
        $"Compartimos un análisis sobre el posible impacto de esta tendencia.",
        $"Exploramos por qué esta tendencia está comenzando a ganar relevancia."
    };

            var closing = new[]
            {
        $"🔎 Leer artículo completo:",
        $"📖 Análisis completo:",
        $"🧠 Más detalles en nuestro blog:",
        $"📊 Conoce el análisis completo:",
        $"👉 Ver artículo completo:"
    };

            var hashtags = new[]
            {
        "#Tecnologia #Software #Innovacion #IGRTEC",
        "#Tecnologia #IA #Software #TransformacionDigital",
        "#Tecnologia #Innovacion #Programacion",
        "#Software #Tecnologia #Digital",
        "#Tecnologia #AI #Innovacion #Software"
    };

            var random = new Random();

            return
        $@"{intro[random.Next(intro.Length)]}

{title}

{context[random.Next(context.Length)]}

{cta[random.Next(cta.Length)]}

{closing[random.Next(closing.Length)]}
{blogUrl}

{hashtags[random.Next(hashtags.Length)]}";
        }

        [HttpGet("test-linkedin")]
        public async Task<IActionResult> TestLinkedin()
        {
            // datos simulados para prueba
            string keyword = "Inteligencia Artificial";
            string title = "¿Qué está pasando con Inteligencia Artificial??";
            string blogUrl = "https://igrtec.com/blog/test";

            // generar mensaje dinámico
            string linkedinMessage = GenerateLinkedInMessage(keyword, title, blogUrl);

            try
            {
                var result = await PublishLinkedIn(linkedinMessage);

                return Ok(new
                {
                    success = true,
                    message = linkedinMessage,
                    linkedinResponse = result
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        private async Task<string> PublishLinkedIn(string message)
        {
            string token = "AQUoyC0G9-G8ON5zW2NvcQI3qHGOi5afITy4ahCaHPNQUKQjtuULyNGUUn7S8cNkUqgIAj9Jy_udM7fOLHym78qOsUZ1vQIcXUmKBQuwpWGuJAMOBc9Rq0svwc1ejyYV3_ecfC_YoWMxZBiDuLlzhOHR1s_bvscrU1r-GCL-Qz_QNnS54fruzcz7TrNxl1fpJIDxGUT2x3Z7WsAQS4BRiSxZpm_aI4QmBT2pqmRK5UheaphxDEch7JrxWW5hqd7rTQ1i5swRhjDrG_JYmO_pSdgNDVHkk6_mDAG5GTHIy-daR7ukX0Cjn8mLESyzQyM3an_cGK02pEc_APO4gnd5hiaWsy6Jsg";

            // El URN debe ser la organización
            string authorUrn = "urn:li:organization:80985944";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            // Usamos una versión estable y soportada
            client.DefaultRequestHeaders.Add("LinkedIn-Version", "202510");
            client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

            var body = new Dictionary<string, object>
{
    // AQUI ESTÁ EL CAMBIO: El author debe ser un objeto, no un string directo
    { "author", "urn:li:organization:80985944" },
    { "lifecycleState", "PUBLISHED" },
    { "specificContent", new Dictionary<string, object>
        {
            { "com.linkedin.ugc.ShareContent", new Dictionary<string, object>
                {
                    { "shareCommentary", new { text = message } },
                    { "shareMediaCategory", "NONE" }
                }
            }
        }
    },
    { "visibility", new Dictionary<string, object>
        {
            { "com.linkedin.ugc.MemberNetworkVisibility", "PUBLIC" }
        }
    }
};
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.linkedin.com/v2/ugcPosts", content);

            return await response.Content.ReadAsStringAsync();
        }

        [HttpGet("get-my-organizations")]
        public async Task<IActionResult> GetMyOrganizations()
        {
            string token = "AQUoyC0G9-G8ON5zW2NvcQI3qHGOi5afITy4ahCaHPNQUKQjtuULyNGUUn7S8cNkUqgIAj9Jy_udM7fOLHym78qOsUZ1vQIcXUmKBQuwpWGuJAMOBc9Rq0svwc1ejyYV3_ecfC_YoWMxZBiDuLlzhOHR1s_bvscrU1r-GCL-Qz_QNnS54fruzcz7TrNxl1fpJIDxGUT2x3Z7WsAQS4BRiSxZpm_aI4QmBT2pqmRK5UheaphxDEch7JrxWW5hqd7rTQ1i5swRhjDrG_JYmO_pSdgNDVHkk6_mDAG5GTHIy-daR7ukX0Cjn8mLESyzQyM3an_cGK02pEc_APO4gnd5hiaWsy6Jsg";

            using var client = new HttpClient();

            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            client.DefaultRequestHeaders.Add("LinkedIn-Version", "202510");
            client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

            // Endpoint para listar organizaciones donde eres admin
            var response = await client.GetAsync("https://api.linkedin.com/rest/organizationAcls?q=roleAssignee");
            var result = await response.Content.ReadAsStringAsync();

            return Ok(new { status = response.StatusCode, response = result });
        }

    //    [HttpGet("linkedin-test-post")]
    //    public async Task<IActionResult> LinkedInTestPost()
    //    {
    //        string token = "AQUoyC0G9-G8ON5zW2NvcQI3qHGOi5afITy4ahCaHPNQUKQjtuULyNGUUn7S8cNkUqgIAj9Jy_udM7fOLHym78qOsUZ1vQIcXUmKBQuwpWGuJAMOBc9Rq0svwc1ejyYV3_ecfC_YoWMxZBiDuLlzhOHR1s_bvscrU1r-GCL-Qz_QNnS54fruzcz7TrNxl1fpJIDxGUT2x3Z7WsAQS4BRiSxZpm_aI4QmBT2pqmRK5UheaphxDEch7JrxWW5hqd7rTQ1i5swRhjDrG_JYmO_pSdgNDVHkk6_mDAG5GTHIy-daR7ukX0Cjn8mLESyzQyM3an_cGK02pEc_APO4gnd5hiaWsy6Jsg";

    //        using var client = new HttpClient();
    //        client.DefaultRequestHeaders.Clear();
    //        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    //        client.DefaultRequestHeaders.Add("LinkedIn-Version", "202401");
    //        client.DefaultRequestHeaders.Add("X-Restli-Protocol-Version", "2.0.0");

    //        // Construimos el JSON manualmente como un objeto de diccionario para preservar los puntos
    //        var body = new Dictionary<string, object>
    //{
    //    { "author", "urn:li:person:X8h1ZSRN6B" },
    //    { "lifecycleState", "PUBLISHED" },
    //    { "specificContent", new Dictionary<string, object>
    //        {
    //            { "com.linkedin.ugc.ShareContent", new Dictionary<string, object>
    //                {
    //                    { "shareCommentary", new { text = "🚀 Prueba LinkedIn API desde IGRTEC4" } },
    //                    { "shareMediaCategory", "NONE" }
    //                }
    //            }
    //        }
    //    },
    //    { "visibility", new Dictionary<string, object>
    //        {
    //            { "com.linkedin.ugc.MemberNetworkVisibility", "PUBLIC" }
    //        }
    //    }
    //};

    //        var json = Newtonsoft.Json.JsonConvert.SerializeObject(body);
    //        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

    //        var response = await client.PostAsync("https://api.linkedin.com/v2/ugcPosts", content);
    //        var result = await response.Content.ReadAsStringAsync();

    //        return Ok(new { status = response.StatusCode, response = result });
    //    }
        [HttpGet("get-my-linkedin-profile")]
        public async Task<IActionResult> GetMyProfile()
        {
            // Asegúrate de que este string tenga el token completo y sin espacios
            string accessToken = "AQUoyC0G9-G8ON5zW2NvcQI3qHGOi5afITy4ahCaHPNQUKQjtuULyNGUUn7S8cNkUqgIAj9Jy_udM7fOLHym78qOsUZ1vQIcXUmKBQuwpWGuJAMOBc9Rq0svwc1ejyYV3_ecfC_YoWMxZBiDuLlzhOHR1s_bvscrU1r-GCL-Qz_QNnS54fruzcz7TrNxl1fpJIDxGUT2x3Z7WsAQS4BRiSxZpm_aI4QmBT2pqmRK5UheaphxDEch7JrxWW5hqd7rTQ1i5swRhjDrG_JYmO_pSdgNDVHkk6_mDAG5GTHIy-daR7ukX0Cjn8mLESyzQyM3an_cGK02pEc_APO4gnd5hiaWsy6Jsg";

            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30); // Aumenta el tiempo de espera a 30 segundos
                                                       // Usamos HttpRequestMessage para tener control total
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.linkedin.com/v2/userinfo");
            // ESTA ES LA FORMA CORRECTA DE AÑADIR EL TOKEN
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            // Encabezados obligatorios para la API de LinkedIn
            request.Headers.Add("LinkedIn-Version", "202602");
            request.Headers.Add("X-Restli-Protocol-Version", "2.0.0");

            var response = await client.SendAsync(request);
            var jsonResult = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                return Ok(jsonResult); // Aquí verás el campo "id"
            }
            else
            {
                return BadRequest(new { status = response.StatusCode, error = jsonResult });
            }

          
        }
        [HttpGet("linkedin/callback")]
        public async Task<IActionResult> LinkedInCallback([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest("Code not received");

            var client = new HttpClient();

            var content = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string,string>("grant_type","authorization_code"),
        new KeyValuePair<string,string>("code", code),
        new KeyValuePair<string,string>("redirect_uri","https://localhost:44315/api/blog/linkedin/callback"),
        new KeyValuePair<string,string>("client_id","78p7c18vix4kbr"),
        new KeyValuePair<string,string>("client_secret","WPL_AP1.cvkYSa02hFx1YYYX.N8lvQA==")
    });

            var response = await client.PostAsync(
                "https://www.linkedin.com/oauth/v2/accessToken",
                content
            );

            var result = await response.Content.ReadAsStringAsync();

            return Ok(result);
        }
        // =========================
        // HELPERS
        // =========================

        private string GenerateSlug(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                return Guid.NewGuid().ToString();

            var slug = title.ToLowerInvariant()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u")
                .Replace("ñ", "n");

            slug = System.Text.RegularExpressions.Regex
                .Replace(slug, @"[^a-z0-9\s-]", "");

            slug = System.Text.RegularExpressions.Regex
                .Replace(slug, @"\s+", " ")
                .Trim()
                .Replace(" ", "-");

            return slug;
        }
    }
}
