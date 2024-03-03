﻿using System.ComponentModel.DataAnnotations.Schema;

namespace SP2024_Assignment3_whsodergren.Models
{
    public class MovieActor
    {
        public int Id { get; set; }

        [ForeignKey("Actor")]
        public int? ActorId { get; set; }
        public Actor? Actor { get; set; }

        
        [ForeignKey("Movie")]
        public int? MovieId { get; set; }
        public Movie? Movie { get; set; }

    }
}
