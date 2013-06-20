﻿using System;
using System.Web.Http.Routing;
using Umbraco.Core;
using umbraco.cms.presentation.Trees;

namespace Umbraco.Web.Trees
{
    /// <summary>
    /// Converts the legacy tree data to the new format
    /// </summary>
    internal class LegacyTreeDataAdapter
    {

        internal static TreeNode ConvertFromLegacy(XmlTreeNode xmlTreeNode, UrlHelper urlHelper)
        {
            //  /umbraco/tree.aspx?rnd=d0d0ff11a1c347dabfaa0fc75effcc2a&id=1046&treeType=content&contextMenu=false&isDialog=false

            //we need to convert the node source to our legacy tree controller
            var source = urlHelper.GetUmbracoApiService<LegacyTreeApiController>("GetNodes");
            //append the query strings
            var query = xmlTreeNode.Source.IsNullOrWhiteSpace()
                ? new string[] { }
                : xmlTreeNode.Source.Split(new[] { '?' }, StringSplitOptions.RemoveEmptyEntries);
            source += query.Length > 1 ? query[1].EnsureStartsWith('?') : "";

            //TODO: Might need to add stuff to additional attributes

            return new TreeNode(xmlTreeNode.NodeID, source)
            {
                HasChildren = xmlTreeNode.HasChildren,
                Icon = xmlTreeNode.Icon,
                Title = xmlTreeNode.Text
            };
        }

        internal static TreeNodeCollection ConvertFromLegacy(XmlTree xmlTree, UrlHelper urlHelper)
        {
            //TODO: Once we get the editor URL stuff working we'll need to figure out how to convert 
            // that over to use the old school ui.xml stuff for these old trees and however the old menu items worked.

            var collection = new TreeNodeCollection();
            foreach (var x in xmlTree.treeCollection)
            {                                
                collection.Add(ConvertFromLegacy(x, urlHelper));
            }
            return collection;
        }

    }
}