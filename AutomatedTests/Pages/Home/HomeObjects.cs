using AutomatedTests.Browser;

namespace AutomatedTests.Pages.Home
{
    public class HomeObjects
    {
        private const string page = "Home";

        public static ObjectBuilder HomeLink = new ObjectBuilder(page, "Home Link", "//a[text()='Home']");
        public static ObjectBuilder WhatWeDoLink = new ObjectBuilder(page, "What We Do Link", "//a[text()='What we do']");
        public static ObjectBuilder HowWeDoItLink = new ObjectBuilder(page, "How We Do It Link", "//a[text()='How we do it']");
        public static ObjectBuilder OurServicesLink = new ObjectBuilder(page, "Our Services Link", "//a[text()='Our Services']");
        public static ObjectBuilder WhyChooseUsLink = new ObjectBuilder(page, "Why Choose Us Link", "//a[text()='Why Choose Us']");
        public static ObjectBuilder WhoWeAreLink = new ObjectBuilder(page, "Who We Are Link", "//a[text()='Who we are']");
        public static ObjectBuilder BlogLink = new ObjectBuilder(page, "Blog Link", "//a[text()='Blog']");
        public static ObjectBuilder ContactUsLink = new ObjectBuilder(page, "Contact Us Link", "//a[text()='contact']");
    }
}
