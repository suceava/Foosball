using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Foosball.Entities
{
	public class Pick
	{
		public const string MASTER_PICKS_USER_ID = "00000000-0000-0000-0000-000000000000";

		public int Id { get; set; }

		[Required]
		public string UserId { get; set; }
		public virtual User User { get; set; }

		[Required]
		public int ScheduleId { get; set; }
		public virtual Schedule Schedule { get; set; }

		public bool PickHomeTeam { get; set; }
		public int CombinedScore { get; set; }
	}
}