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



        /// <summary>
        /// initialize the sqllite database
        /// </summary>
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

        /// <summary>
        /// returns the database directory from settings
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Seed all tables
        /// </summary>
        /// <param name="connection"></param>
        private void _createTables(SQLiteConnection connection)
        {
            connection.CreateTable<Youtube_Video>();
            connection.CreateTable<Thumbnail>();
        }

        /// <summary>
        /// Adds New Video to Database
        /// </summary>
        /// <param name="video"></param>
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

        /// <summary>
        /// adds new Thumbnail
        /// </summary>
        /// <param name="thumbnail"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Delete a video
        /// </summary>
        /// <param name="video"></param>
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

        /// <summary>
        /// return all videos ids 
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// search for videos by names
        /// </summary>
        /// <param name="ToMatch"></param>
        /// <returns></returns>
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

        /// <summary>
        /// fetch all videos in local youtube videos infos 
        /// </summary>
        /// <param name="startIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
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
