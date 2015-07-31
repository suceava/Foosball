using Foosball.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foosball.Models
{
	public class PickViewModel
	{
		public int Id { get; set; }
		public UserListViewModel User { get; set; }
		public ScheduleViewModel Schedule { get; set; }
		public bool PickHomeTeam { get; set; }
		public int ScoreDifferential { get; set; }

		#region conversion

		public static PickViewModel FromPick(Pick pick)
		{
			return new PickViewModel()
			{
				Id = pick.Id,
				User = UserListViewModel.FromUser(pick.User),
				Schedule = ScheduleViewModel.FromSchedule(pick.Schedule),
				PickHomeTeam = pick.PickHomeTeam,
				ScoreDifferential = pick.ScoreDifferential
			};
		}

		#endregion
	}
}