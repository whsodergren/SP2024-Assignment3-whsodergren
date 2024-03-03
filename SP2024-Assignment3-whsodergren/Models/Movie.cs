using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP2024_Assignment3_whsodergren.Models
{
    public class Movie
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }

        [Display(Name ="Year of Release")]
        public string YearOfRelease { get; set; }

        [Display(Name ="IMBD Page")]
        public string IMDBHyperlink { get; set; }

        [DataType(DataType.Upload)]
        [DisplayName("Movie Image")]
        public byte[]? MovieImage { get; set; }

    }
}
