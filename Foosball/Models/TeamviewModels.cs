using Foosball.DataContexts;
using Foosball.Entities;
using LinqKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Foosball.Models
{
	public class TeamViewModel
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Location { get; set; }
		public string Division { get; set; }
		public string ImageUrl { get; set; }

		#region conversion

		public static TeamViewModel FromTeam(Team team)
		{
			return new TeamViewModel()
			{
				Id = team.Id,
				Name = team.Name,
				Location = team.Location,
				Division = team.Division,
				ImageUrl = team.ImageUrl
			};
        }

		public Team ToTeam()
		{
			return new Team()
			{
				Id = Id,
				Name = Name,
				Location = Location,
				Division = Division,
				ImageUrl = ImageUrl
			};
		}

		#endregion

		public static TeamViewModel Get(int id)
		{
			if (id == 0)
			{
				return new TeamViewModel();
			}

			return GetFilteredList(id: id).FirstOrDefault();
		}

		public static List<TeamViewModel> GetList()
		{
			return GetFilteredList();
		}

		private static List<TeamViewModel> GetFilteredList(int? id = null)
		{
			var predicate = PredicateBuilder.True<Team>();
			if (id.HasValue)
			{
				predicate = predicate.And(s => s.Id == id.Value);
			}

			using (var db = new TeamsDb())
			{
				return db.Teams.AsExpandable().Where(predicate).OrderBy(t => t.Name).ToList().Select(t => TeamViewModel.FromTeam(t)).ToList();
			}
		}

	}
}