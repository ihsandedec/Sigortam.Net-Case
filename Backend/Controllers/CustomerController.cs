using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CustomerController : ControllerBase
{
    private readonly SigortamNetDbContext _context;

    public CustomerController(SigortamNetDbContext context)
    {
        _context = context;
    }

    private async Task<bool> KpsDogrula(string tckn, string ad, string soyad, DateTime dogumTarihi)
    {
        try
        {
            using (var client = new HttpClient())
            {
                var dogumYili = dogumTarihi.Year;

                
                var soapXml = $@"<?xml version=""1.0"" encoding=""utf-8""?>
                <soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/"">
                    <soap:Body>
                        <TCKimlikNoDogrula xmlns=""http://tckimlik.nvi.gov.tr/WS"">
                            <TCKimlikNo>{tckn}</TCKimlikNo>
                            <Ad>{ad.ToUpper()}</Ad>
                            <Soyad>{soyad.ToUpper()}</Soyad>
                            <DogumYili>{dogumYili}</DogumYili>
                        </TCKimlikNoDogrula>
                    </soap:Body>
                </soap:Envelope>";

                var content = new StringContent(soapXml, System.Text.Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://tckimlik.nvi.gov.tr/WS/TCKimlikNoDogrula");

                var response = await client.PostAsync("https://tckimlik.nvi.gov.tr/service/kpspublic.asmx", content);
                var responseContent = await response.Content.ReadAsStringAsync();

                
                return responseContent.Contains("<TCKimlikNoDogrulaResult>true</TCKimlikNoDogrulaResult>");
            }
        }
        catch
        {
            
            return false;
        }
    }


    [HttpPost]
    public async Task<IActionResult> SaveOrUpdate([FromBody] CustomerRequestDto dto)
    {
        var adSoyadRegex = new Regex("^[a-zA-ZçÇğĞıİöÖşŞüÜ]+$");

        if (string.IsNullOrWhiteSpace(dto.Tckn) || dto.Tckn.Length != 11 || !dto.Tckn.All(char.IsDigit))
            return BadRequest("TCKN 11 haneli ve numerik olmalı.");

        if (string.IsNullOrWhiteSpace(dto.Ad) || dto.Ad.Length > 50 || !adSoyadRegex.IsMatch(dto.Ad))
            return BadRequest("Ad sadece harf içermeli ve 50 karakteri geçmemelidir.");


        if (string.IsNullOrWhiteSpace(dto.Soyad) || dto.Soyad.Length > 50 || !adSoyadRegex.IsMatch(dto.Soyad))
            return BadRequest("Soyad sadece harf içermeli ve 50 karakteri geçmemelidir.");

        if (dto.DogumTarihi >= DateTime.Today)
            return BadRequest("Doğum tarihi bugünden küçük olmalı.");

        if (!await KpsDogrula(dto.Tckn, dto.Ad, dto.Soyad, dto.DogumTarihi))
            return BadRequest("KPS doğrulaması başarısız.");
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Tckn == dto.Tckn);

        if (customer == null)
        {
            customer = new Customer
            {
                Tckn = dto.Tckn,
                Ad = dto.Ad,
                Soyad = dto.Soyad,
                DogumTarihi = dto.DogumTarihi.ToString("yyyy-MM-dd"),
                IsActive = 1,
                CreatedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
            };
            _context.Customers.Add(customer);
        }
        else
        {
            customer.Ad = dto.Ad;
            customer.Soyad = dto.Soyad;
            customer.DogumTarihi = dto.DogumTarihi.ToString("yyyy-MM-dd");
            customer.IsActive = 1;
        }

        await _context.SaveChangesAsync();
        return Ok(customer);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        if (customer == null)
            return NotFound();
        return Ok(customer);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string? ad, [FromQuery] string? soyad, [FromQuery] string? tckn)
    {
        var query = _context.Customers.Where(x => x.IsActive == 1);

        if (!string.IsNullOrWhiteSpace(ad))
            query = query.Where(x => x.Ad.Contains(ad));
        if (!string.IsNullOrWhiteSpace(soyad))
            query = query.Where(x => x.Soyad.Contains(soyad));
        if (!string.IsNullOrWhiteSpace(tckn))
            query = query.Where(x => x.Tckn.Contains(tckn));

        var results = await query.ToListAsync();

        var masked = results.Select(x => new CustomerMaskedDto
        {

            Id = x.Id,
            Tckn = "*******" + x.Tckn.Substring(7, 4),
            Ad = x.Ad.Length > 2 ? x.Ad.Substring(0, 2) + new string('*', 5) : x.Ad,
            Soyad = x.Soyad.Length > 2 ? x.Soyad.Substring(0, 2) + new string('*', 5) : x.Soyad,
            DogumTarihi = "**/**/" + x.DogumTarihi.Substring(0, 4),
            IsActive = x.IsActive == 1
        }).ToList();
        return Ok(masked);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var customer = await _context.Customers.FirstOrDefaultAsync(x => x.Id == id && x.IsActive == 1);
        if (customer == null)
            return NotFound();
        customer.IsActive = 0;
        await _context.SaveChangesAsync();
        return NoContent();
    }

}

// DTO
public class CustomerRequestDto
{
    public string Tckn { get; set; } = null!;
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public DateTime DogumTarihi { get; set; }
}

public class CustomerMaskedDto
{
    public int Id { get; set; }
    public string Tckn { get; set; } = null!;
    public string Ad { get; set; } = null!;
    public string Soyad { get; set; } = null!;
    public string DogumTarihi { get; set; } = null!;
    public bool IsActive { get; set; }
}