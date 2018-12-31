using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using static CitizenFX.Core.BaseScript;

namespace Sumo
{
    /// <summary>
    /// Grid Item/card.
    /// </summary>
    public struct Item
    {
        public string title;
        public string description;
        public string textureDict;
        public string textureName;
        public bool selected;
        public int votes;
        public bool enabled;
    }

    /// <summary>
    /// Grid page struct. Used to keep track of all important data on a page.
    /// </summary>
    public struct GridPage
    {
        public Item[] items;
        public int currentHover;
        public bool isFirstPage;
        public bool isLastPage;
        public int selected;
    }

    static class GridSelector
    {
        public static List<GridPage> GridPages { get; private set; } = new List<GridPage>();
        public static int currentPage { get; private set; } = -1;
        public static KeyValuePair<int, int> lastSelection { get; private set; } = new KeyValuePair<int, int>(0, 0); // key: page, value: selected index.
        public static int currentHoverIndex { get; private set; } = 0;

        public static Scaleform _scale = new Scaleform("MP_NEXT_JOB_SELECTION");

        /// <summary>
        /// Adds a new page to the GridSelector window.
        /// </summary>
        /// <param name="names">A list of the 6 grid item titles/names.</param>
        /// <param name="descriptions">A list of the 6 grid item descriptions.</param>
        /// <param name="textureDicts">A list of the 6 grid item texture dictionary names.</param>
        /// <param name="textureNames">A list of the 6 grid item texture names.</param>
        /// <param name="isFirstPage">Make this the first page, and disable the "previous page" button.</param>
        /// <param name="isLastPage">Make this the last page, and disable the "next page" button.</param>
        public static void AddPage(string[] names, string[] descriptions, string[] textureDicts, string[] textureNames, bool isFirstPage, bool isLastPage)
        {
            if (names.Count() < 6 || descriptions.Count() < 6 || textureDicts.Count() < 6 || textureNames.Count() < 6)
            {
                Debug.WriteLine("Not enough items for this page. Please ensure all (6) required grid items have names, descriptions and a texture dict/name.");
            }
            else
            {
                var items = new Item[9];
                for (var i = 0; i < 6; i++)
                {
                    items[i] = new Item()
                    {
                        title = names[i],
                        description = descriptions[i],
                        selected = false,
                        votes = 0,
                        textureDict = textureDicts[i],
                        textureName = textureNames[i],
                        enabled = true
                    };
                }
                items[6] = new Item()
                {
                    title = "Previous Page",
                    description = "Go to the previous page.",
                    enabled = !isFirstPage,
                    selected = false,
                    textureDict = "",
                    textureName = "",
                    votes = 0
                };
                items[7] = new Item()
                {
                    title = "Confirm Selection",
                    description = "Confirm your selected vehicle and mark yourself as ready, this will lock your selection.",
                    enabled = true,
                    selected = false,
                    textureDict = "",
                    textureName = "",
                    votes = 0
                };
                items[8] = new Item()
                {
                    title = "Next Page",
                    description = "Go to the next page.",
                    enabled = !isLastPage,
                    selected = false,
                    textureDict = "",
                    textureName = "",
                    votes = 0
                };
                GridPages.Add(new GridPage()
                {
                    currentHover = 0,
                    isFirstPage = isFirstPage,
                    isLastPage = isLastPage,
                    selected = -1,
                    items = items
                });
            }
        }

        /// <summary>
        /// Pre-loads the (new) page and sets up all images and grid titles. This is required before a page can be shown.
        /// </summary>
        /// <param name="pageIndex">The page to (re)load.</param>
        /// <param name="selectedIndex">The index of the item to select (add checkmark + 1 vote) on this page.</param>
        /// <param name="hoverIndex">The index of the item you want to highlight this page.</param>
        /// <param name="maxPages">The total amount of pages to display in the top right corner.</param>
        /// <returns></returns>
        public static async Task LoadPage(int pageIndex, int selectedIndex, int hoverIndex, int maxPages)
        {
            _scale.CallFunction("CLEANUP_MOVIE");
            //_scale.Dispose();

            //Scaleform newScaleformHandle = new Scaleform("MP_NEXT_JOB_SELECTION");
            //if (!newScaleformHandle.IsLoaded)
            //{
            //    RequestScaleformMovie("MP_NEXT_JOB_SELECTION");
            //    while (newScaleformHandle.IsLoaded)
            //    {
            //        await Delay(0);
            //    }
            //}
            _scale.CallFunction("SET_TITLE", "Sumo - Select A Vehicle", $"Page {pageIndex + 1}/{maxPages}");
            var page = GridPages[pageIndex];
            for (var i = 0; i < 9; i++)
            {
                var item = page.items[i];
                if (item.textureDict != "" && item.textureName != "")
                {
                    if (!HasStreamedTextureDictLoaded(item.textureDict))
                    {
                        RequestStreamedTextureDict(item.textureDict, false);
                        while (!HasStreamedTextureDictLoaded(item.textureDict))
                        {
                            await Delay(0);
                        }
                    }
                }
                if (i < 6)
                {
                    _scale.CallFunction("SET_GRID_ITEM", i, GetVehicleName(item.title), item.textureDict, item.textureName, 0,
                        0, -1, false, 0.0, 0.0, !item.enabled, -1);
                }
                else
                {
                    _scale.CallFunction("SET_GRID_ITEM", i, item.title, item.textureDict, item.textureName, -1, 0, -2, false,
                        0.0, 0.0, !item.enabled, -1);
                }
            }
            if (selectedIndex > -1 && selectedIndex < 9)
            {
                _scale.CallFunction("SET_GRID_ITEM_VOTE", selectedIndex, 1, 0, true, true);
            }

            if (selectedIndex < 6)
            {
                _scale.CallFunction("SET_SELECTION", hoverIndex, GetVehicleName(GridPages[pageIndex].items[hoverIndex].title),
                    GridPages[pageIndex].items[hoverIndex].description, false);
            }
            else
            {
                _scale.CallFunction("SET_SELECTION", hoverIndex, GridPages[pageIndex].items[hoverIndex].title,
                    GridPages[pageIndex].items[hoverIndex].description, false);
            }

            //_scale = newScaleformHandle;
            return;
        }

        /// <summary>
        /// Shows the page provided by pageIndex.
        /// </summary>
        /// <param name="pageIndex">The page to show.</param>
        /// <param name="maxPages">Used to update the max pages on the top right corner.</param>
        /// <returns></returns>
        public static async Task ShowPage(int pageIndex, int maxPages)
        {
            if (GridPages.Count() < pageIndex)
            {
                Debug.WriteLine("Page does not exist.");
            }
            else
            {
                if (currentPage != pageIndex)
                {
                    if (lastSelection.Key == pageIndex)
                    {
                        await LoadPage(pageIndex, lastSelection.Value, currentHoverIndex, maxPages);
                    }
                    else
                    {
                        await LoadPage(pageIndex, -1, currentHoverIndex, maxPages);
                    }

                    currentPage = pageIndex;
                }
                if (!_scale.IsLoaded)
                {
                    RequestScaleformMovie("MP_NEXT_JOB_SELECTION");
                    while (_scale.IsLoaded)
                    {
                        await Delay(0);
                    }
                }
                if (currentHoverIndex < 6)
                {
                    _scale.CallFunction("SET_SELECTION", currentHoverIndex, GetVehicleName(GridPages[pageIndex].items[currentHoverIndex].title),
                        GridPages[pageIndex].items[currentHoverIndex].description, false);
                }
                else
                {
                    _scale.CallFunction("SET_SELECTION", currentHoverIndex, GridPages[pageIndex].items[currentHoverIndex].title,
                        GridPages[pageIndex].items[currentHoverIndex].description, false);
                }
                if (currentPage == lastSelection.Key)
                {
                    for (var i = 0; i < 9; i++)
                    {
                        _scale.CallFunction("SET_GRID_ITEM_VOTE", i, 0, 0, false, false);
                    }
                    _scale.CallFunction("SET_GRID_ITEM_VOTE", lastSelection.Value, 1, 0, true, false);
                }
                else
                {
                    if (lastSelection.Value == 7)
                    {
                        for (var i = 0; i < 9; i++)
                        {
                            _scale.CallFunction("SET_GRID_ITEM_VOTE", i, 0, 0, false, false);
                        }
                        _scale.CallFunction("SET_GRID_ITEM_VOTE", lastSelection.Value, 1, 0, true, false);
                    }
                }

                _scale.Render2D();
            }
        }

        /// <summary>
        /// Returns the localized vehicle name for the given vehicle model name.
        /// </summary>
        /// <param name="modelName">The vehicle model name to get the localized name for.</param>
        /// <returns></returns>
        public static string GetVehicleName(string modelName)
        {
            return GetLabelText(GetDisplayNameFromVehicleModel((uint)GetHashKey(modelName)));
        }

        /// <summary>
        /// Moves one item to the right. If already at the most right item, it will go to the far left item on the same row.
        /// </summary>
        public static void GoRight()
        {
            PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);

            var newval = currentHoverIndex + 1;
            if (newval == 3)
            {
                currentHoverIndex = 0;
            }
            else if (newval == 6)
            {
                currentHoverIndex = 3;
            }
            else if (newval == 9)
            {
                currentHoverIndex = 6;
            }
            else
            {
                currentHoverIndex = newval;
            }
        }

        /// <summary>
        /// Moves one item to the left. If already at the most left item, it will go to the far right item on the same row.
        /// </summary>
        public static void GoLeft()
        {
            PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            var newval = currentHoverIndex - 1;
            if (newval == -1)
            {
                currentHoverIndex = 2;
            }
            else if (newval == 2)
            {
                currentHoverIndex = 5;
            }
            else if (newval == 5)
            {
                currentHoverIndex = 8;
            }
            else
            {
                currentHoverIndex = newval;
            }
        }

        /// <summary>
        /// Moves one step up. If already at the top, then it'll go to the bottom of the same column.
        /// </summary>
        public static void GoUp()
        {
            PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            var newval = currentHoverIndex - 3;
            if (newval < 0)
            {
                currentHoverIndex = 9 + newval;
            }
            else
            {
                currentHoverIndex = newval;
            }
        }

        /// <summary>
        /// Moves one step down. If already at the bottom, then it'll go to the top of the same column.
        /// </summary>
        public static void GoDown()
        {
            PlaySoundFrontend(-1, "NAV_UP_DOWN", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            var newval = currentHoverIndex + 3;
            if (newval > 8)
            {
                currentHoverIndex = newval - 9;
            }
            else
            {
                currentHoverIndex = newval;
            }
        }

        /// <summary>
        /// Selects the grid item that is currently being highlighted.
        /// </summary>
        public static async void Select()
        {
            PlaySoundFrontend(-1, "SELECT", "HUD_FRONTEND_DEFAULT_SOUNDSET", false);
            if (currentHoverIndex != 8 && currentHoverIndex != 6 && currentHoverIndex != 7)
            {
                _scale.CallFunction("SET_GRID_ITEM_VOTE", currentHoverIndex, 0, 0, true, true);
                lastSelection = new KeyValuePair<int, int>(currentPage, currentHoverIndex);
            }
            else if (currentHoverIndex != 7)
            {
                if (currentPage < GridPages.Count() - 1 && currentHoverIndex == 8)
                {
                    await ShowPage(currentPage + 1, GridPages.Count());

                }
                if (currentPage > 0 && GridPages.Count() > 1 && currentHoverIndex == 6)
                {
                    await ShowPage(currentPage - 1, GridPages.Count());
                }
            }
        }

        /// <summary>
        /// Returns the raw name of the currently selected grid item.
        /// </summary>
        /// <returns></returns>
        public static string GetSelectedItemName()
        {
            return GridPages[lastSelection.Key].items[lastSelection.Value].title;
        }
    }
}
