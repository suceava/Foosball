using Foosball.DataContexts;
using Foosball.Entities;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Foosball.Models
{
	public class AnnouncementViewModel
	{
		public string Announcement { get; set; }

		public static AnnouncementViewModel Get()
		{
			using (var db = new SystemSettingsDb())
			{
				var settings = db.SystemSettings.FirstOrDefault();

				return new AnnouncementViewModel
				{
					Announcement = (settings != null ? settings.Announcement : null)
                };
			}
		}

		public void Save()
		{
			using (var db = new SystemSettingsDb())
			{
				var settings = db.SystemSettings.FirstOrDefault();
				if (settings == null)
				{
					settings = new SystemSettings();
					db.Entry(settings).State = EntityState.Added;
				}
				else
				{
					db.Entry(settings).State = EntityState.Modified;
				}
				settings.Announcement = Announcement;

				db.SaveChanges();
			}
		}
	}
}