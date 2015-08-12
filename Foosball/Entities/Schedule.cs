using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace Foosball.Entities
{
	public class Schedule
	{
		public int Id { get; set; }

		[Required]
		public int Week { get; set; }
		[Required]
		public DateTime Date { get; set; }
		[Required]
		public int HomeTeamId { get; set; }
		public virtual Team HomeTeam { get; set; }
		[Required]
		public int AwayTeamId { get; set; }
		public virtual Team AwayTeam { get; set; }
		public bool RequireScore { get; set; }
		public bool IsPickable { get; set; }
	}
}