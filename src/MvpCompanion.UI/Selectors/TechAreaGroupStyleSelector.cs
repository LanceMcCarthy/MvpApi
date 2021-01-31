﻿using Windows.UI.Xaml.Controls;

namespace MvpCompanion.UI.Selectors
{
    internal class TechAreaGroupStyleSelector : GroupStyleSelector
    {
        public GroupStyle TechnologyAreaGroupStyle { get; set; }
        
        protected override GroupStyle SelectGroupStyleCore(object @group, uint level)
        {
            return TechnologyAreaGroupStyle;
        }
    }
}