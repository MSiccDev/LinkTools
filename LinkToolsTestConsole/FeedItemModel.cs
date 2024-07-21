using System;
using MSiccDev.Libs.LinkTools.LinkPreview;

namespace LinkToolsTestConsole;

public class FeedItemModel
{
    public required string Title { get; set; }
        
    public required Uri Link { get; set; }
        
    public string? Description { get; set; }
        
    public DateTimeOffset? PublishedAt { get; set; }
        
    public Uri? ImageUrl { get; set; }
    public bool IsRead { get; set; }
        
    public bool IsBookMarked { get; set; }
    public required string FeedTitle { get; set; }
        
    public Uri? FeedImageUrl { get; set; }
        
}

public class FeedItemModelWithLinkPreviewResponse : FeedItemModel
{
    public LinkPreviewRequest? LinkPreviewRequest { get; set; }
}