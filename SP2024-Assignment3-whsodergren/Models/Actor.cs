using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP2024_Assignment3_whsodergren.Models
{
    public class Actor
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }

        [Display(Name = "IMBD Page")]
        public string IMDBHyperlink { get; set; }


        [DataType(DataType.Upload)]
        [DisplayName("Actor Image")]
        public byte[]? ActorImage { get; set; }

    }
}
