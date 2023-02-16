# RSS feed aggregator

Create an application that aggregates RSS feeds from various developer sites.

For example, you can use the following RSS feeds:

1. [https://stackoverflow.blog/feed/](https://stackoverflow.blog/feed/)
2. [https://dev.to/feed](https://dev.to/feed)
3. [https://www.freecodecamp.org/news/rss](https://www.freecodecamp.org/news/rss)
4. [https://martinfowler.com/feed.atom](https://martinfowler.com/feed.atom)
5. [https://codeblog.jonskeet.uk/feed/](https://codeblog.jonskeet.uk/feed/)
6. [https://devblogs.microsoft.com/visualstudio/feed/](https://devblogs.microsoft.com/visualstudio/feed/)
7. [https://feed.infoq.com/](https://feed.infoq.com/)
8. [https://css-tricks.com/feed/](https://css-tricks.com/feed/)
9. [https://codeopinion.com/feed/](https://codeopinion.com/feed/)
10. [https://andrewlock.net/rss.xml](https://andrewlock.net/rss.xml)
11. [https://michaelscodingspot.com/index.xml](https://michaelscodingspot.com/index.xml)
12. [https://www.tabsoverspaces.com/feed.xml](https://www.tabsoverspaces.com/feed.xml)

The application should constantly check rss feeds and if there is a new article, it should add information about the article to the database (article link, title, short description, author, image, tags, publication date).

- When adding an article to the database, all text fields must be checked and, if any, javascript code must be removed.
- When adding an article to the database, the title or description should be checked if it contains tags already added to the database. If it contains, you should add all such tags to the article (for example, if the description of the article says: "`Last week saw the release of the third preview in the lead up to the official release of Visual Studio for Mac 17.5`" and there is a tag "Visual Studio", then you should automatically add the tag "Visual Studio" to this article.) The number of automatically generated tags should not exceed 5. [optional]
- Articles with the same title should not be added from one site
- Each RSS feed should be processed in parallel mode

The application should also have an API that allows users to view aggregated articles (chronologically, using paging).

It should also be possible to extract articles by tag using the API.