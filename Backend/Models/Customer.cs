using System;
using System.Collections.Generic;

namespace Backend.Models;

public partial class Customer
{
    public int Id { get; set; }

    public string Tckn { get; set; } = null!;

    public string Ad { get; set; } = null!;

    public string Soyad { get; set; } = null!;

    public string DogumTarihi { get; set; } = null!;

    public int IsActive { get; set; }

    public string CreatedDate { get; set; } = null!;
}
