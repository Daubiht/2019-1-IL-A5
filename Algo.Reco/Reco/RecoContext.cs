using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Algo
{
    public class RecoContext
    {
        public IReadOnlyList<User> Users { get; private set; }
        public IReadOnlyList<Movie> Movies { get; private set; }
        public int RatingCount { get; private set; }

        public bool LoadFrom(string folder)
        {
            string p = Path.Combine(folder, "users.dat");
            if (!File.Exists(p)) return false; 
            Users = User.ReadUsers(p);
            p = Path.Combine(folder, "movies.dat");
            if (!File.Exists(p)) return false;
            Movies = Movie.ReadMovies(p);
            p = Path.Combine(folder, "ratings.dat");
            if (!File.Exists(p)) return false;
            RatingCount = User.ReadRatings(Users, Movies, p);
            return true;
        }

        /// <summary>
        /// Get distance between two users based on what they like and do not like
        /// </summary>
        /// <param name="u1"></param>
        /// <param name="u2"></param>
        /// <returns>percent of proximity : 100% indentical, 0% nothing in common</returns>
        public double Distance(User u1, User u2)
        {
            var MoviesCommon = u1.Ratings.Where( e => u2.Ratings.ContainsKey( e.Key ) );

            if( MoviesCommon.Count() > 0)
                return 0.0;

            //Movies they both like
            var MoviesLikedUser1 = u1.Ratings.Where( m => m.Value > 4 );
            var MoviesLikedUser2 = u2.Ratings.Where( m => m.Value > 4 );

            var MoviesLikedCommon = MoviesLikedUser1.Intersect( MoviesLikedUser2 );

            //Movies they both don't like
            var MoviesNotLikedUser1 = u1.Ratings.Where( m => m.Value > 2 );
            var MoviesNotLikedUser2 = u2.Ratings.Where( m => m.Value > 2 );

            var MoviesNotLikedCommon = MoviesNotLikedUser1.Intersect( MoviesNotLikedUser2 );

            double distance = MoviesCommon.Count() / ( MoviesLikedCommon.Count() + MoviesNotLikedCommon.Count()) * 100;

            return distance;
        }

        public double SimilarityPearson(User u1, User u2)
        {
            var MoviesCommon = u1.Ratings.Where(e => u2.Ratings.ContainsKey(e.Key));
            List<(int x, int y)> values = new List<(int x, int y)>();

            foreach(KeyValuePair<Movie, int> rate in MoviesCommon)
            {
                values.Add( (rate.Value, u2.Ratings.Where(e => rate.Key == e.Key).First().Value) );
            }

            return SimilarityPearson( values );
        }

        public double SimilarityPearson(IEnumerable<(int x, int y)> values)
        {
            double sx = 0.0;
            double sy = 0.0;
            double sxx = 0.0;
            double syy = 0.0;
            double sxy = 0.0;

            int n = values.Count();

            foreach((int x, int y) item in values)
            {
                double x = item.x;
                double y = item.y;

                sx += x;
                sy += y;
                sxx += x * x;
                syy += y * y;
                sxy += x * y;
            }

            // covariation
            double cov = sxy / n - sx * sy / n / n;
            // standard error of x
            double sigmax = Math.Sqrt( sxx / n - sx * sx / n / n );
            // standard error of y
            double sigmay = Math.Sqrt( syy / n - sy * sy / n / n );

            // correlation is just a normalized covariation
            return cov / sigmax / sigmay;
        }
    }
}
