using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Articulate.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Articulate
{
    public class ArticulateTagsRouteHandler : UmbracoVirtualNodeByIdRouteHandler
    {
        private struct UrlAndPageNames
        {
            public int NodeId { get; set; }
            public string TagsUrlName { get; set; }
            public string TagsPageName { get; set; }
            public string CategoriesUrlName { get; set; }
            public string CategoriesPageName { get; set; }
        }

        private readonly List<UrlAndPageNames> _urlsAndPageNames = new List<UrlAndPageNames>();

        [Obsolete("Use the ctor with all dependencies instead")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ArticulateTagsRouteHandler(IEnumerable<IPublishedContent> itemsForRoute)
            : this(UmbracoContext.Current.UrlProvider, itemsForRoute)
        {
        }


        /// <summary>
        /// Constructor used to create a new handler for multi-tenency with domains and ids
        /// </summary>
        /// <param name="itemsForRoute"></param>
        public ArticulateTagsRouteHandler(UrlProvider umbracoUrlProvider, IEnumerable<IPublishedContent> itemsForRoute)
            : base(umbracoUrlProvider, itemsForRoute)
        {
            foreach (var node in itemsForRoute)
            {
                _urlsAndPageNames.Add(new UrlAndPageNames
                {
                    NodeId = node.Id,
                    TagsUrlName = node.GetPropertyValue<string>("tagsUrlName"),
                    TagsPageName = node.GetPropertyValue<string>("tagsPageName"),
                    CategoriesUrlName = node.GetPropertyValue<string>("categoriesUrlName"),
                    CategoriesPageName = node.GetPropertyValue<string>("categoriesPageName")
                });
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public ArticulateTagsRouteHandler(int realNodeId,
            string tagsUrlName,
            string tagsPageName,
            string categoriesUrlName,
            string categoriesPageName)
            : base(realNodeId)
        {
            _urlsAndPageNames.Add(new UrlAndPageNames
            {
                CategoriesPageName = categoriesPageName,
                CategoriesUrlName = categoriesUrlName,
                NodeId = realNodeId,
                TagsPageName = tagsPageName,
                TagsUrlName = tagsUrlName
            });
        }

        protected override IPublishedContent FindContent(RequestContext requestContext, UmbracoContext umbracoContext, IPublishedContent baseContent)
        {
            var urlAndPageName = _urlsAndPageNames.Single(x => x.NodeId == baseContent.Id);

            var tag = HttpUtility.UrlDecode(requestContext.RouteData.Values["tag"] == null ? null : requestContext.RouteData.Values["tag"].ToString());
            var actionName = requestContext.RouteData.GetRequiredString("action");
            var rootUrl = baseContent.Url;
            var urlName = actionName.InvariantEquals("tags") ? urlAndPageName.TagsUrlName : urlAndPageName.CategoriesUrlName;
            var pageName = actionName.InvariantEquals("tags") ? urlAndPageName.TagsPageName : urlAndPageName.CategoriesPageName;

            return new ArticulateVirtualPage(
                baseContent,
                tag.IsNullOrWhiteSpace() ? pageName : tag,
                requestContext.RouteData.GetRequiredString("controller"),
                tag.IsNullOrWhiteSpace()
                    ? urlName
                    : urlName.EnsureEndsWith('/') + tag);
        }
    }

}