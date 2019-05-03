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
            var MoviesCommon = u1.Ratings.Intersect( u2.Ratings );

            if(MoviesCommon.Count() > 0)
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
    }
}
