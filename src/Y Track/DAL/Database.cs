using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Y_Track.DAL.Models;

namespace Y_Track.DAL
{
    // TODO :: Change this from singleton to a regular disposable class
    public class Database
    {
        private string _databasePath;

        private static Database _instance;

        public static Database Instance
        {
            get
            {
                if (_instance == null) _instance = new Database();
                return _instance;
            }
        }


        protected Database()
        {
            _initializeDatabase();
        }


        private void _initializeDatabase()
        {


            _databasePath = Path.Combine(_initializeDatabaseOutputDirectory(), "y.db");

            // create database if not exist or use the exiting one
            if (!File.Exists(_databasePath))
            {
                var connection = new SQLiteConnection(_databasePath);
                _createTables(connection);
                connection.Close();
            }
        }


        private string _initializeDatabaseOutputDirectory()
        {
            var dbOutputDirectory = Properties.Settings.Default.DatabaseDirectory;
            if (!Directory.Exists(dbOutputDirectory))
            {
                Properties.Settings.Default.DatabaseDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                Properties.Settings.Default.Save();
            }
            return Properties.Settings.Default.DatabaseDirectory;
        }


        private void _createTables(SQLiteConnection connection)
        {
            connection.CreateTable<Youtube_Video>();
            connection.CreateTable<Thumbnail>();
        }


        public void AddVideo(Youtube_Video video)
        {
            try
            {
                using (var connection = new SQLiteConnection(_databasePath))
                {
                    connection.InsertOrReplace(video);
                }
            }
            catch (Exception e)
            {
                Helpers.YTrackLogger.Log("Cannot store video " + video.Id + " into database\n\n" + e.StackTrace);
            }
        }

        public int? AddThumbnail(Thumbnail thumbnail)
        {
            try
            {
                using (var connection = new SQLiteConnection(_databasePath))
                {

                    connection.Insert(thumbnail);
                    return connection.ExecuteScalar<int>("SELECT last_insert_rowid()");
                }
            }
            catch (Exception e)
            {
                Helpers.YTrackLogger.Log("Cannot store video thumbnail " + thumbnail.Id + " into database\n\n" + e.StackTrace);
                return null;
            }
        }

        public void DeleteVideo(Youtube_Video video)
        {
            try
            {
                using (var connection = new SQLiteConnection(_databasePath))
                {
                    connection.Delete<Thumbnail>(video.ThumbnailId);
                    connection.Delete(video);
                }
            }
            catch (Exception e)
            {
                Helpers.YTrackLogger.Log("Cannot delete video  " + video.Id + " from the database\n\n" + e.StackTrace);

            }
        }

        public async Task<List<string>> GetAllVideosIds()
        {
            try
            {
                using (var connection = new SQLiteConnection(_databasePath))
                {
                    var sql = $"SELECT Id FROM Youtube_Video";
                    List<string> videos = connection.Query<Youtube_Video>(sql).Select(x => x.Id).ToList();
                    return await Task.FromResult(videos);
                }
            }
            catch (Exception e)
            {
                Helpers.YTrackLogger.Log("Cannot Fetch Videos Ids From Database \n\n" + e.StackTrace);
                return null;
            }
        }


        public async Task<List<Youtube_Video>> GetVideosLike(string ToMatch)
        {
            try
            {
                using (var connection = new SQLiteConnection(_databasePath))
                {
                    var sql = $"SELECT * FROM Youtube_Video WHERE Description LIKE '%{ToMatch}%' OR Author LIKE '%{ToMatch}%' OR TITLE LIKE '%{ToMatch}%'";
                    List<Youtube_Video> videos = connection
                        .Query<Youtube_Video>(sql);
                    foreach (var video in videos)
                    {
                        var thumbnail = connection.Query<Thumbnail>("SELECT * FROM Thumbnail WHERE Id=?", video.ThumbnailId).FirstOrDefault();
                        video.Thumbnail = thumbnail;
                    }
                    return await Task.FromResult(videos);
                }
            }
            catch (Exception e)
            {
                Helpers.YTrackLogger.Log("Cannot read videos from local database\n\n" + e.StackTrace);
                return null;
            }
        }

        public async Task<List<Youtube_Video>> ReadLocalYoutubeVideosInfo(int startIndex, int pageSize)
        {
            try
            {
                using (var connection = new SQLiteConnection(_databasePath))
                {
                    List<Youtube_Video> videos = connection.Query<Youtube_Video>("SELECT * FROM Youtube_Video LIMIT " + pageSize + " OFFSET " + startIndex);
                    foreach (var video in videos)
                    {
                        var thumbnail = connection.Query<Thumbnail>("SELECT * FROM Thumbnail WHERE Id=?", video.ThumbnailId).FirstOrDefault();
                        video.Thumbnail = thumbnail;
                    }
                    return await Task.FromResult(videos);
                }
            }
            catch (Exception e)
            {
                Helpers.YTrackLogger.Log("Cannot read videos from local database\n\n" + e.StackTrace);
                return null;
            }
        }


    }
}
