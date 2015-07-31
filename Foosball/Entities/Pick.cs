using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Foosball.Entities
{
	public class Pick
	{
		public int Id { get; set; }

		[Required]
		public string UserId { get; set; }
		public virtual User User { get; set; }

		[Required]
		public int ScheduleId { get; set; }
		public virtual Schedule Schedule { get; set; }

		public bool PickHomeTeam { get; set; }
		public int ScoreDifferential { get; set; }
	}
}