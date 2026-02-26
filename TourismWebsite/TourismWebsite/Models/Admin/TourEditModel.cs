using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TourismServer.Models.Admin;

public sealed class TourEditModel
{
    public string Title { get; set; } = "";
    public string PriceText { get; set; } = "";
    public string DurationText { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public bool IsTop { get; set; }
}