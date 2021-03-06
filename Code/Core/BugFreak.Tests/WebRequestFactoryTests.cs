﻿using System;
using System.Net;
using BugFreak.Components;
using NUnit.Framework;

namespace BugFreak.Tests
{
    [TestFixture]
    public class WebRequestFactoryTests
    {
        private WebRequestFactory _subject;

        [SetUp]
        public void SetUp()
        {
            _subject = new WebRequestFactory();
        }

        [Test]
        public void Create_Always_ReturnsHttpWebRequestInstance()
        {
            var result = _subject.Create(new Uri("http://google.com"));

            Assert.IsTrue(result is HttpWebRequest);
        }
    }
}