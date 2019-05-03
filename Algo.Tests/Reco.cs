using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Runtime.CompilerServices;

namespace Algo.Tests
{
    [TestFixture]
    public class Reco
    {
        static string GetMovieDataPath( [CallerFilePath]string thisFilePath = null )
        {
            string algoPath = Path.GetDirectoryName( Path.GetDirectoryName( thisFilePath ) );
            return Path.Combine( algoPath, "ThirdParty", "MovieData" );
        }

        static string GetBadDataPath() => Path.Combine( GetMovieDataPath(), "MovieLens" );

        static string GetGoodDataPath() => GetMovieDataPath();

        RecoContext _context;

        [SetUp]
        public void LoadTestDataOnlyOnce()
        {
            if( _context == null )
            {
                var c = new RecoContext();
                if( c.LoadFrom( GetGoodDataPath() ) ) _context = c;
            }
        }

        [Test]
        public void corrects_raw_data_from_ThirdParty_MovieData_MovieLens_to_ThirdParty_MovieData()
        {
            Dictionary<int, List<Movie>> duplicateMovies;
            Dictionary<int, Movie> firstMovies = Movie.ReadMovies( Path.Combine( GetBadDataPath(), "movies.dat" ), out duplicateMovies );
            int idMovieMin = firstMovies.Keys.Min();
            int idMovieMax = firstMovies.Keys.Max();
            Console.WriteLine( "{3} Movies from {0} to {1}, {2} duplicates.", idMovieMin, idMovieMax, duplicateMovies.Count, firstMovies.Count );

            Dictionary<int, List<User>> duplicateUsers;
            Dictionary<int, User> firstUsers = User.ReadUsers( Path.Combine( GetBadDataPath(), "users.dat" ), out duplicateUsers );
            int idUserMin = firstUsers.Keys.Min();
            int idUserMax = firstUsers.Keys.Max();
            Console.WriteLine( "{3} Users from {0} to {1}, {2} duplicates.", idUserMin, idUserMax, duplicateUsers.Count, firstUsers.Count );

            Dictionary<int, string> badLines;
            int nbRating = User.ReadRatings( Path.Combine( GetBadDataPath(), "ratings.dat" ), firstUsers, firstMovies, out badLines );
            Console.WriteLine( "{0} Ratings: {1} bad lines.", nbRating, badLines.Count );

            Directory.CreateDirectory( GetGoodDataPath() );
            // Saves Movies
            using( TextWriter w = File.CreateText( Path.Combine( GetGoodDataPath(), "movies.dat" ) ) )
            {
                int idMovie = 0;
                foreach( Movie m in firstMovies.Values )
                {
                    m.MovieId = ++idMovie;
                    w.WriteLine( "{0}::{1}::{2}", m.MovieId, m.Title, String.Join( "|", m.Categories ) );
                }
            }

            // Saves Users
            string[] occupations = new string[]{
                "other",
                "academic/educator",
                "artist",
                "clerical/admin",
                "college/grad student",
                "customer service",
                "doctor/health care",
                "executive/managerial",
                "farmer",
                "homemaker",
                "K-12 student",
                "lawyer",
                "programmer",
                "retired",
                "sales/marketing",
                "scientist",
                "self-employed",
                "technician/engineer",
                "tradesman/craftsman",
                "unemployed",
                "writer" };
            using( TextWriter w = File.CreateText( Path.Combine( GetGoodDataPath(), "users.dat" ) ) )
            {
                int idUser = 0;
                foreach( User u in firstUsers.Values )
                {
                    u.UserID = ++idUser;
                    string occupation;
                    int idOccupation;
                    if( int.TryParse( u.Occupation, out idOccupation )
                        && idOccupation >= 0
                        && idOccupation < occupations.Length )
                    {
                        occupation = occupations[idOccupation];
                    }
                    else occupation = occupations[0];
                    w.WriteLine( "{0}::{1}::{2}::{3}::{4}", u.UserID, u.Male ? 'M' : 'F', u.Age, occupation, "US-" + u.ZipCode );
                }
            }
            // Saves Rating
            using( TextWriter w = File.CreateText( Path.Combine( GetGoodDataPath(), "ratings.dat" ) ) )
            {
                foreach( User u in firstUsers.Values )
                {
                    foreach( var r in u.Ratings )
                    {
                        w.WriteLine( "{0}::{1}::{2}", u.UserID, r.Key.MovieId, r.Value );
                    }
                }
            }
        }

        [Test]
        public void dump_counts_and_check_that_UserId_and_MovieId_are_one_based()
        {
            Console.WriteLine( $"{_context.Users.Count} users, {_context.Movies.Count} movies, {_context.RatingCount} ratings." );
            for( int i = 0; i < _context.Users.Count; ++i )
                Assert.That( _context.Users[i].UserID, Is.EqualTo( i + 1 ) );
            for( int i = 0; i < _context.Movies.Count; ++i )
                Assert.That( _context.Movies[i].MovieId, Is.EqualTo( i + 1 ) );

        }
    }
}
