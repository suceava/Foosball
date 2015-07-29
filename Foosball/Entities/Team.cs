using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Foosball.Entities
{
	public class Team
	{
		public int Id { get; set; }

		[Required]
		[StringLength(100)]
		public string Name { get; set; }
		[Required]
		[StringLength(100)]
		public string Location { get; set; }
		[StringLength(100)]
		public string Division { get; set; }
		[StringLength(1000)]
		public string ImageUrl { get; set; }
	}
}