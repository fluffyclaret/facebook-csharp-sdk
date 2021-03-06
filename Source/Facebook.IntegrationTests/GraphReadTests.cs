﻿// --------------------------------
// <copyright file="GraphReadTests.cs" company="Facebook C# SDK">
//     Microsoft Public License (Ms-PL)
// </copyright>
// <author>Nathan Totten (ntotten.com) and Jim Zimmerman (jimzimmerman.com)</author>
// <license>Released under the terms of the Microsoft Public License (Ms-PL)</license>
// <website>http://facebooksdk.codeplex.com</website>
// ---------------------------------

namespace Facebook.Tests.Graph
{
    using System;
    using System.Configuration;
    using System.Dynamic;
    using Xunit;

    /*
        * All objects in Facebook can be accessed in the same way:

       •Users: https://graph.facebook.com/btaylor (Bret Taylor)
       •Pages: https://graph.facebook.com/cocacola (Coca-Cola page)
       •Events: https://graph.facebook.com/251906384206 (Facebook Developer Garage Austin)
       •Groups: https://graph.facebook.com/2204501798 (Emacs users group)
       •Applications: https://graph.facebook.com/2439131959 (the Graffiti app)
       •Status messages: https://graph.facebook.com/367501354973 (A status message from Bret)
       •Photos: https://graph.facebook.com/98423808305 (A photo from the Coca-Cola page)
       •Photo albums: https://graph.facebook.com/99394368305 (Coca-Cola's wall photos)
       •Profile pictures: http://graph.facebook.com/jimizim/picture (your profile picture)
       •Videos: https://graph.facebook.com/614004947048 (A Facebook tech talk on Tornado)
       •Notes: https://graph.facebook.com/122788341354 (Note announcing Facebook for iPhone 3.0)

        */

    public class GraphReadTests
    {

        private FacebookClient app;
        public GraphReadTests()
        {
            app = new FacebookClient();
            app.AccessToken = ConfigurationManager.AppSettings["AccessToken"];
        }

        [Fact]
        public void Read_Likes()
        {
            dynamic likesResult = app.Get("/totten/likes");
            dynamic likesData = likesResult.data;
            Assert.NotNull(likesData);

            dynamic total = likesData.Count;
            Assert.NotEqual(0, total);

            var firstLikePageName = likesData[0].name;
            Assert.NotEqual(String.Empty, firstLikePageName);
        }

        [Fact]
        public void Read_Public_Fan_Page_Id()
        {
            dynamic pageResult = app.Get("/outback");
            Assert.Equal(pageResult.id, "48543634386");
        }

        [Fact]
        public void Read_User_Info()
        {
            dynamic result = app.Get("/me");
            Assert.Equal(result.name, "Nathan Tester");
        }

        [Fact]
        public void Read_Application_Info()
        {
            dynamic result = app.Get("/2439131959");
            Assert.Equal(result.category, "Just For Fun");
        }

        [Fact]
        public void Read_Photo_Info()
        {
            dynamic result = app.Get("/98423808305");
            Assert.Equal(result.from.name, "Coca-Cola");
        }

        [Fact]
        public void Read_Event()
        {
            dynamic result = app.Get("/331218348435");
            Assert.Equal(result.venue.city, "Austin");
        }

        [Fact]
        public void ReadPublicProfile()
        {
            dynamic result = app.Get("/totten");
            Assert.Equal("Nathan", result.first_name);
        }

        [Fact]
        public void get_user_likes_should_throw_oauth()
        {
            dynamic parameters = new ExpandoObject();
            parameters.access_token = "invalidtoken";

            Assert.Throws<FacebookOAuthException>(
                () =>
                    {
                        dynamic result = app.Get("/totten/likes", parameters);
                    });
        }
    }
}
