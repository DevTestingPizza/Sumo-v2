using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CitizenFX.Core.UI;

namespace Sumo
{
    class Gfx : BaseScript
    {
        public static bool DisableDrawing = false;
        public static bool LimboActive = false;

        /// <summary>
        /// Draws text on screen.
        /// </summary>
        /// <param name="text">The text to display on screen.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="size">The font size.</param>
        public static void DrawText(string text, float x, float y, float size)
        {
            DrawText(text, x, y, size, Font.ChaletComprimeCologne, Alignment.Left, false, -1, 255);
        }
        /// <summary>
        /// Draws text on screen.
        /// </summary>
        /// <param name="text">The text to display on screen.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="size">The font size.</param>
        /// <param name="font">The font to use.</param>
        public static void DrawText(string text, float x, float y, float size, Font font)
        {
            DrawText(text, x, y, size, font, Alignment.Left, false, -1, 255);
        }
        /// <summary>
        /// Draws text on screen.
        /// </summary>
        /// <param name="text">The text to display on screen.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="size">The font size.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="align">Text alignment (center, left, right).</param>
        public static void DrawText(string text, float x, float y, float size, Font font, Alignment align)
        {
            DrawText(text, x, y, size, font, align, false, -1, 255);
        }
        /// <summary>
        /// Draws text on screen.
        /// </summary>
        /// <param name="text">The text to display on screen.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="size">The font size.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="align">Text alignment (center, left, right).</param>
        /// <param name="outlined">Outline the text with a black border.</param>
        public static void DrawText(string text, float x, float y, float size, Font font, Alignment align, bool outlined)
        {
            DrawText(text, x, y, size, font, align, outlined, -1, 255);
        }
        /// <summary>
        /// Draws text on screen.
        /// </summary>
        /// <param name="text">The text to display on screen.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="size">The font size.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="align">Text alignment (center, left, right).</param>
        /// <param name="outlined">Outline the text with a black border.</param>
        /// <param name="duration">Display time in miliseconds. Set to -1 to draw only one frame.</param>
        public static void DrawText(string text, float x, float y, float size, Font font, Alignment align, bool outlined, int duration)
        {
            DrawText(text, x, y, size, font, align, outlined, duration, 255);
        }
        /// <summary>
        /// Draws text on screen.
        /// </summary>
        /// <param name="text">The text to display on screen.</param>
        /// <param name="x">The x position.</param>
        /// <param name="y">The y position.</param>
        /// <param name="size">The font size.</param>
        /// <param name="font">The font to use.</param>
        /// <param name="align">Text alignment (center, left, right).</param>
        /// <param name="outlined">Outline the text with a black border.</param>
        /// <param name="duration">Display time in miliseconds. Set to -1 to draw only one frame.</param>
        public static async void DrawText(string text, float x, float y, float size, Font font, Alignment align, bool outlined, int duration, int opacity)
        {
            if (IsHudPreferenceSwitchedOn())
            {
                SetTextColour(255, 255, 255, opacity);
                var strings = StringToArray(text);
                if (duration == -1)
                {
                    SetTextFont((int)font);
                    SetTextScale(1f, (size * 27f) / Screen.Resolution.Height); // always pixel perfect text height in pixels.
                    SetTextJustification((int)align);
                    if (align == Alignment.Right)
                    {
                        SetTextWrap(0f, x);
                    }
                    if (outlined)
                    {
                        SetTextOutline();
                    }
                    BeginTextCommandDisplayText("THREESTRINGS");
                    foreach (string sentence in strings)
                    {
                        AddTextComponentSubstringPlayerName(sentence);
                    }

                    if (align == Alignment.Right)
                    {
                        EndTextCommandDisplayText(0f, y);
                    }
                    else
                    {
                        EndTextCommandDisplayText(x, y);
                    }
                }
                else
                {
                    var timer = GetGameTimer();
                    while (GetGameTimer() - timer <= duration)
                    {
                        await Delay(0);
                        SetTextFont((int)font);
                        SetTextScale(1f, size);
                        SetTextJustification((int)align);
                        if (align == Alignment.Right)
                        {
                            SetTextWrap(0f, x);
                        }
                        if (outlined)
                        {
                            SetTextOutline();
                        }
                        BeginTextCommandDisplayText("THREESTRINGS");
                        foreach (string sentence in strings)
                        {
                            AddTextComponentSubstringPlayerName(sentence);
                        }
                        if (align == Alignment.Right)
                        {
                            EndTextCommandDisplayText(0f, y);
                        }
                        else
                        {
                            EndTextCommandDisplayText(x, y);
                        }
                    }
                }
            }

        }

        #region StringToStringArray
        /// <summary>
        /// Converts an input string into 1, 2 or 3 strings all stacked in a string[3].
        /// Use this to convert text into multiple smaller pieces to be used in functions like 
        /// drawing text, drawing help messages or drawing notifications on screen.
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string[] StringToArray(string inputString)
        {
            string[] outputString = new string[3];

            var lastSpaceIndex = 0;
            var newStartIndex = 0;
            outputString[0] = inputString;

            if (inputString.Length > 99)
            {
                for (int i = 0; i < inputString.Length; i++)
                {
                    if (inputString.Substring(i, 1) == " ")
                    {
                        lastSpaceIndex = i;
                    }

                    if (inputString.Length > 99 && i >= 98)
                    {
                        if (i == 98)
                        {
                            outputString[0] = inputString.Substring(0, lastSpaceIndex);
                            newStartIndex = lastSpaceIndex + 1;
                        }
                        if (i > 98 && i < 198)
                        {
                            if (i == 197)
                            {
                                outputString[1] = inputString.Substring(newStartIndex, (lastSpaceIndex - (outputString[0].Length - 1))
                                    - (inputString.Length - 1 > 197 ? 1 : -1));
                                newStartIndex = lastSpaceIndex + 1;
                            }
                            else if (i == inputString.Length - 1 && inputString.Length < 198)
                            {
                                outputString[1] = inputString.Substring(newStartIndex, ((inputString.Length - 1) - outputString[0].Length));
                                newStartIndex = lastSpaceIndex + 1;
                            }
                        }
                        if (i > 197)
                        {
                            if (i == inputString.Length - 1 || i == 296)
                            {
                                outputString[2] = inputString.Substring(newStartIndex, ((inputString.Length - 1) - outputString[0].Length)
                                    - outputString[1].Length);
                            }
                        }
                    }
                }
            }

            return outputString;
        }
        #endregion

        /// <summary>
        /// Get the correct x coord for drawing on screen based on real pixel coord input.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static float GetX(float x)
        {
            return x / Screen.Resolution.Width;
        }
        /// <summary>
        /// Get the correct y coord for drawing on screen based on real pixel coord input.
        /// </summary>
        /// <param name="y"></param>
        /// <returns></returns>
        public static float GetY(float y)
        {
            return y / Screen.Resolution.Height;
        }

        /// <summary>
        /// Draws a score/stats bar on the lower right of your screen.
        /// </summary>
        /// <param name="leftText">The text that should appear on the left side.</param>
        /// <param name="rightText">The text that should appear on the right side.</param>
        /// <param name="rowIndex">The row index. 0 = bottom bar, rowIndex > 0 is any row above.</param>
        public static async void DrawStatBar(string leftText, string rightText, int rowIndex)
        {
            if (IsHudPreferenceSwitchedOn() && !DisableDrawing)
            {
                // Hide overlapping info.
                HideHudComponentThisFrame((int)HudComponent.AreaName);
                HideHudComponentThisFrame((int)HudComponent.StreetName);
                HideHudComponentThisFrame((int)HudComponent.VehicleName);

                // Loading textures
                var dict = "timerbars";
                var texture = "all_black_bg";
                if (!HasStreamedTextureDictLoaded(dict))
                {
                    RequestStreamedTextureDict(dict, false);
                }
                while (!HasStreamedTextureDictLoaded(dict))
                {
                    await Delay(0);
                }

                // Base width, height and positions onscreen.
                float width = 400f / (float)Screen.Resolution.Width;
                float height = 40f / (float)Screen.Resolution.Height;
                float posx = ((float)Screen.Resolution.Width - (400f / 2f) - 20f) / (float)Screen.Resolution.Width;
                float posy = ((float)Screen.Resolution.Height - (40f / 2f) - 20f - ((45f * (float)rowIndex))) / (float)Screen.Resolution.Height;

                // Background.
                DrawSprite(dict, texture, posx, posy, width, height, 0f, 0, 0, 0, 200);

                // Left text
                posx = posx - ((40f / (float)Screen.Resolution.Width));
                posy = posy - (height / 2f - (6f / (float)Screen.Resolution.Height));
                DrawText(leftText, posx, posy, 13f, Font.ChaletLondon, Alignment.Right, false, -1, 230);

                // Right text/dot sprite.
                if (rightText != "(player)")
                {
                    posx = ((float)Screen.Resolution.Width - (400f / 2f) - 20f) / (float)Screen.Resolution.Width;
                    posx = posx + (width / 2f - (6f / (float)Screen.Resolution.Width));
                    posy = posy = ((float)Screen.Resolution.Height - (40f / 2f) - 20f - ((45f * (float)rowIndex))) / (float)Screen.Resolution.Height;
                    posy = posy - (height / 2f + (3.6f / (float)Screen.Resolution.Height));
                    DrawText(rightText, posx, posy, 23f, Font.ChaletLondon, Alignment.Right, false, -1, 180);
                }
                else
                {
                    var r = 255;
                    var g = 255;
                    var b = 255;
                    var a = 255;
                    GetHudColour(27 + rowIndex, ref r, ref g, ref b, ref a);
                    posx = ((float)Screen.Resolution.Width - (400f / 2f) - 20f) / (float)Screen.Resolution.Width;
                    posx = posx + (width / 2f - (25f / (float)Screen.Resolution.Width));
                    posy = posy = ((float)Screen.Resolution.Height - (40f / 2f) - 20f - ((45f * (float)rowIndex))) / (float)Screen.Resolution.Height;
                    DrawSprite(dict, "circle_checkpoints", posx, posy, 25f / (float)Screen.Resolution.Width, 25f / (float)Screen.Resolution.Height, 0f, r, g, b, a);
                }
            }
        }

        /// <summary>
        /// Shows the end round animation. (TODO) 
        /// </summary>
        /// <returns></returns>
        public static async Task ShowIntroScaleform()
        {
            var bg = new Scaleform("MP_CELEBRATION_BG");
            var fg = new Scaleform("MP_CELEBRATION_FG");
            var cb = new Scaleform("MP_CELEBRATION");
            RequestScaleformMovie("MP_CELEBRATION_BG");
            RequestScaleformMovie("MP_CELEBRATION_FG");
            RequestScaleformMovie("MP_CELEBRATION");
            while (!bg.IsLoaded || !fg.IsLoaded || !cb.IsLoaded)
            {
                await Delay(0);
            }

            // Setting up colors.
            bg.CallFunction("CREATE_STAT_WALL", "ch", "HUD_COLOUR_BLACK", -1);
            fg.CallFunction("CREATE_STAT_WALL", "ch", "HUD_COLOUR_RED", -1);
            cb.CallFunction("CREATE_STAT_WALL", "ch", "HUD_COLOUR_BLUE", -1);

            // Setting up pause duration.
            bg.CallFunction("SET_PAUSE_DURATION", 3.0f);
            fg.CallFunction("SET_PAUSE_DURATION", 3.0f);
            cb.CallFunction("SET_PAUSE_DURATION", 3.0f);

            bool won = new Random().Next(0, 2) == 0;
            string win_lose = won ? "CELEB_LOSER" : "CELEB_WINNER";

            bg.CallFunction("ADD_WINNER_TO_WALL", "ch", win_lose, GetPlayerName(PlayerId()), "", 0, false, "", false);
            fg.CallFunction("ADD_WINNER_TO_WALL", "ch", win_lose, GetPlayerName(PlayerId()), "", 0, false, "", false);
            cb.CallFunction("ADD_WINNER_TO_WALL", "ch", win_lose, GetPlayerName(PlayerId()), "", 0, false, "", false);

            // Setting up background.
            bg.CallFunction("ADD_BACKGROUND_TO_WALL", "ch");
            fg.CallFunction("ADD_BACKGROUND_TO_WALL", "ch");
            cb.CallFunction("ADD_BACKGROUND_TO_WALL", "ch");

            // Preparing to show the wall.
            bg.CallFunction("SHOW_STAT_WALL", "ch");
            fg.CallFunction("SHOW_STAT_WALL", "ch");
            cb.CallFunction("SHOW_STAT_WALL", "ch");

            // Drawing the wall on screen for 3 seconds + 1 seconds (for outro animation druation).
            var timer = GetGameTimer();
            DisableDrawing = true;
            while (GetGameTimer() - timer <= (3000 + 1000))
            {
                await Delay(0);
                DrawScaleformMovieFullscreenMasked(bg.Handle, fg.Handle, 255, 255, 255, 255);
                DrawScaleformMovieFullscreen(cb.Handle, 255, 255, 255, 255, 0);
                HideHudAndRadarThisFrame();
            }
            DisableDrawing = false;

            // Playing effect when it's over.
            StartScreenEffect("MinigameEndNeutral", 0, false);
            PlaySoundFrontend(-1, "SCREEN_FLASH", "CELEBRATION_SOUNDSET", false);

            // Cleaning up.
            bg.CallFunction("CLEANUP");
            fg.CallFunction("CLEANUP");
            cb.CallFunction("CLEANUP");

            bg.Dispose();
            fg.Dispose();
            cb.Dispose();
        }

        /// <summary>
        /// Toggles the limbo/lobby overlay on/off.
        /// </summary>
        public static async void ToggleLimbo()
        {
            //if (IsGameplayCamRendering())
            if (!LimboActive)
            {
                var pos = Game.PlayerPed.Position;
                var cam = CreateCam("DEFAULT_SCRIPTED_CAMERA", true);
                SetCamCoord(cam, pos.X, pos.Y, pos.Z + 200f);
                //PointCamAtCoord(cam, pos.X, pos.Y, pos.Z);
                SetCamRot(cam, 0f, 0f, 0f, 0);
                RenderScriptCams(true, true, 2500, true, false);
                SetCamActive(cam, true);
                await Delay(0);
                SetEntityVisible(PlayerPedId(), false, false);
                SetEntityInvincible(PlayerPedId(), true);
                FreezeEntityPosition(PlayerPedId(), true);
                //SetEntityCollision(PlayerPedId(), false, false);
                TransitionToBlurred(2500f);
                LimboActive = true;
                DrawLobbyOverlay();
                //SetFrontendActive(true);
            }
            else
            {
                SetGameplayCamRelativeHeading(0f);
                RenderScriptCams(false, true, 2500, false, false);
                DestroyAllCams(true);
                SetEntityVisible(PlayerPedId(), true, true);
                SetEntityInvincible(PlayerPedId(), false);
                FreezeEntityPosition(PlayerPedId(), false);
                TransitionFromBlurred(2500f);

                //var blip = GetMainPlayerBlipId();
                //SetBlipAsFriendly(blip, false);
                //SetBlipColour(blip, 6 + PlayerId());
                //SetBlipHighDetail(blip, true);
                //SetBlipFriend(blip, false);
                //SetBlipBright(blip, true);
                //PulseBlip(blip);
                //ShowHeadingIndicatorOnBlip(blip, false);
                //Debug.Write(DoesBlipExist(blip) + "\n");
            }
        }

        /// <summary>
        /// Draws the limbo/lobby overlay for selecting a car.
        /// </summary>
        public static async void DrawLobbyOverlay()
        {
            #region previous testing
            /*
            RequestScaleformMovie("MP_NEXT_JOB_SELECTION");
            Scaleform sc = new Scaleform("MP_NEXT_JOB_SELECTION");
            while (!sc.IsLoaded)
            {
                await Delay(0);
            }
            sc.CallFunction("SET_TITLE", "Sumo - Choose A Vehicle", "Page 1/2");

            //string sTXD = "sssa_default";
            //string sTXN = "hexer";
            //string sTXD = "mphud";
            //string sTXN = "missionpassedgradient";

            //int textureLoadType = 0;
            //int verifiedType = 0;
            ////int eIcon = 2; // 2 = race
            //int eIcon = -1; // 2 = race
            //bool bCheck = false;
            //float rpMult = 0;
            //float cashMult = 0;
            //bool bDisabled = false;
            //int iconCol = 0;

            var grids = new List<grid>()
            {
                //new grid(){title = "Surano", description = "", textureDict = "sssa_default", textureName = "surano", votes = 0},
                //new grid(){title = "Vacca", description = "", textureDict = "sssa_default", textureName = "vacca", votes = 0},
                //new grid(){title = "Sandking", description = "", textureDict = "sssa_default", textureName = "sandking", votes = 0},
                //new grid(){title = "Adder", description = "", textureDict = "lgm_default", textureName = "adder", votes = 0},
                //new grid(){title = "Entity XF", description = "", textureDict = "lgm_default", textureName = "entityxf", votes = 0},
                //new grid(){title = "Hotknife", description = "", textureDict = "lgm_default", textureName = "hotknife", votes = 0 },
                new grid(){title = "Previous Page", description = "View the previous page.", textureDict = "", textureName = "", votes = 0 },
                new grid(){title = "Random Vehicle", description = "Choose a random vehicle.", textureDict = "", textureName = "", votes = 0 },
                new grid(){title = "Next Page", description = "View the next page.", textureDict = "", textureName = "", votes = 0 },
            };

            VehicleSelector.AddPage(new VehicleCard[6]
                {
                VehicleSelector.Vehicles[0],
                VehicleSelector.Vehicles[1],
                VehicleSelector.Vehicles[2],
                VehicleSelector.Vehicles[3],
                VehicleSelector.Vehicles[4],
                VehicleSelector.Vehicles[5]
                }, sc, 0);


            //sc.CallFunction("SET_GRID_ITEM", 0, grids[0].title, grids[0].textureDict, grids[0].textureName, textureLoadType, verifiedType, eIcon, bCheck, rpMult, cashMult, bDisabled, iconCol);
            //sc.CallFunction("SET_GRID_ITEM", 1, grids[1].title, grids[1].textureDict, grids[1].textureName, textureLoadType, verifiedType, eIcon, bCheck, rpMult, cashMult, bDisabled, iconCol);
            //sc.CallFunction("SET_GRID_ITEM", 2, grids[2].title, grids[2].textureDict, grids[2].textureName, textureLoadType, verifiedType, eIcon, bCheck, rpMult, cashMult, bDisabled, iconCol);
            //sc.CallFunction("SET_GRID_ITEM", 3, grids[3].title, grids[3].textureDict, grids[3].textureName, textureLoadType, verifiedType, eIcon, bCheck, rpMult, cashMult, bDisabled, iconCol);
            //sc.CallFunction("SET_GRID_ITEM", 4, grids[4].title, grids[4].textureDict, grids[4].textureName, textureLoadType, verifiedType, eIcon, bCheck, rpMult, cashMult, bDisabled, iconCol);
            //sc.CallFunction("SET_GRID_ITEM", 5, grids[5].title, grids[5].textureDict, grids[5].textureName, textureLoadType, verifiedType, eIcon, bCheck, rpMult, cashMult, bDisabled, iconCol);
            sc.CallFunction("SET_GRID_ITEM", 6, grids[0].title, "", "", -1, 0, -2, false, 0f, 0f, true, -1);
            sc.CallFunction("SET_GRID_ITEM", 7, grids[1].title, "", "", -1, 0, -2, false, 0f, 0f, false, -1);
            sc.CallFunction("SET_GRID_ITEM", 8, grids[2].title, "", "", -1, 0, -2, false, 0f, 0f, false, -1);

            var selected = 0;
            DisableDrawing = true;
            var currentPage = 1;
            while (LimboActive)
            {

                await Delay(0);
                sc.Render2D();
                if (Game.IsDisabledControlJustPressed(0, Control.FrontendRight))
                {
                    selected++;
                    if (selected > 8)
                    {
                        selected = 0;
                    }
                }
                if (Game.IsDisabledControlJustPressed(0, Control.FrontendLeft))
                {
                    selected--;
                    if (selected < 0)
                    {
                        selected = 8;
                    }
                }
                if (Game.IsDisabledControlJustPressed(0, Control.FrontendUp))
                {
                    //if (selected >= 0 && selected <= 2)
                    //{
                    //    selected++;
                    //}
                    selected -= 3;
                    if (selected < 0)
                    {
                        selected = 9 + selected;
                    }
                }
                if (Game.IsDisabledControlJustPressed(0, Control.FrontendDown))
                {
                    selected += 3;
                    if (selected > 8)
                    {
                        selected = selected - 9;
                    }
                }
                if (Game.IsDisabledControlJustPressed(0, Control.PhoneSelect))
                {
                    if (selected == 8 && currentPage == 1)
                    {
                        sc.Dispose();
                        sc = new Scaleform("MP_NEXT_JOB_SELECTION");
                        VehicleSelector.AddPage(new VehicleCard[6]
                        {
                            VehicleSelector.Vehicles[6],
                            VehicleSelector.Vehicles[7],
                            VehicleSelector.Vehicles[8],
                            VehicleSelector.Vehicles[9],
                            VehicleSelector.Vehicles[10],
                            VehicleSelector.Vehicles[11]
                        }, sc, 0);
                        //selected = 0;
                        sc.CallFunction("SET_GRID_ITEM", 6, grids[0].title, "", "", -1, 0, -2, false, 0f, 0f, false, -1);
                        sc.CallFunction("SET_GRID_ITEM", 7, grids[1].title, "", "", -1, 0, -2, false, 0f, 0f, false, -1);
                        sc.CallFunction("SET_GRID_ITEM", 8, grids[2].title, "", "", -1, 0, -2, false, 0f, 0f, true, -1);
                        currentPage = 2;

                        sc.CallFunction("SET_TITLE", "Sumo - Choose A Vehicle", "Page 2/2");
                    }
                    else if (selected == 6 && currentPage == 2)
                    {
                        sc.Dispose();
                        sc = new Scaleform("MP_NEXT_JOB_SELECTION");
                        VehicleSelector.AddPage(new VehicleCard[6]
                        {
                            VehicleSelector.Vehicles[0],
                            VehicleSelector.Vehicles[1],
                            VehicleSelector.Vehicles[2],
                            VehicleSelector.Vehicles[3],
                            VehicleSelector.Vehicles[4],
                            VehicleSelector.Vehicles[5]
                        }, sc, 0);
                        //selected = 0;
                        sc.CallFunction("SET_GRID_ITEM", 6, grids[0].title, "", "", -1, 0, -2, false, 0f, 0f, true, -1);
                        sc.CallFunction("SET_GRID_ITEM", 7, grids[1].title, "", "", -1, 0, -2, false, 0f, 0f, false, -1);
                        sc.CallFunction("SET_GRID_ITEM", 8, grids[2].title, "", "", -1, 0, -2, false, 0f, 0f, false, -1);
                        currentPage = 1;
                        sc.CallFunction("SET_TITLE", "Sumo - Choose A Vehicle", "Page 1/2");
                    }
                    else if (!((selected == 8 || selected == 6) && currentPage == 1) && !((selected == 6 || selected == 8) && currentPage == 2))
                    {
                        //grids[selected] = new grid() { title = grids[selected].title, description = grids[selected].description, votes = grids[selected].votes + 1 };
                        for (int i = 0; i < 9; i++)
                        {
                            sc.CallFunction("SET_GRID_ITEM_VOTE", i, 0, 1, false, false);
                        }
                        sc.CallFunction("SET_GRID_ITEM_VOTE", selected, 1, 1, true, true);
                    }

                }

                if (currentPage == 1)
                {
                    sc.CallFunction("SET_SELECTION", selected, selected < 6 ? VehicleSelector.GetVehicleName(VehicleSelector.Vehicles[selected].ModelName) : grids[selected - 6].title,
                    selected < 6 ? "" : grids[selected - 6].description, false);
                }
                else
                {
                    sc.CallFunction("SET_SELECTION", selected, selected < 6 ? VehicleSelector.GetVehicleName(VehicleSelector.Vehicles[selected + 6].ModelName) : grids[selected - 6].title,
                    selected < 6 ? "" : grids[selected - 6].description, false);
                }

                //sc.CallFunction("SET_HOVER", selected, false);
                //sc.CallFunction("SET_LOBBY_LIST_VISIBILITY", true);
                sc.CallFunction("SET_DETAILS_ITEM", 7);
                HideHudAndRadarThisFrame();
            }
            DisableDrawing = false;
            sc.Dispose();

            //while (true)
            //{
            //    //HideHudAndRadarThisFrame();
            //    DisableDrawing = true;
            //    await Delay(0);
            //    if (LimboActive)
            //    {
            //        if (opacity < 125)
            //        {
            //            opacity += 1;
            //        }
            //    }
            //    else
            //    {
            //        if (opacity > 0)
            //        {
            //            opacity -= 1;
            //        }
            //        else
            //        {
            //            DisableDrawing = false;
            //            t.Dispose();
            //            break;
            //        }
            //    }
            //    //t.Render2D();
            //    //DrawRect(0.5f, 0.5f, 1200f / Class1.ScreenWidth, 800f / Class1.ScreenHeight, 5, 5, 5, opacity);
            //    //var r = 0;
            //    //var g = 0;
            //    //var b = 0;
            //    //var a = 0;
            //    //GetHudColour(63, ref r, ref g, ref b, ref a);
            //    //DrawRect(0.5f, GetY((float)Screen.Resolution.Height / 2f - 800f / 2f - 35f), 1200f / Class1.ScreenWidth, 55f / Screen.Resolution.Height, 20, 91, 173, opacity);
            //    //DrawRect(0.5f, GetY((float)Screen.Resolution.Height / 2f - 800f / 2f - 35f), 1200f / Class1.ScreenWidth, 55f / Screen.Resolution.Height, r, g, b, opacity);
            //    //DrawText("Sumo", GetX((float)Screen.Resolution.Width / 2f - 1200f / 2f + 5f), GetY((float)Screen.Resolution.Height / 2f - 800f / 2f - 62f), 32f, Font.HouseScript, Alignment.Left, false, -1, opacity + 50);

            //}
            */
            #endregion

            if (!(GridSelector.GridPages.Count() > 0))
            {
                Debug.WriteLine("Adding pages");
                GridSelector.AddPage
                (
                    names: new string[6] { "adder", "banshee", "bullet", "carbonizzare", "cheetah", "coquette" },
                    descriptions: new string[6] { "", "", "", "", "", "" },
                    textureDicts: new string[6] { "lgm_default", "lgm_default", "lgm_default", "lgm_default", "lgm_default", "lgm_default" },
                    textureNames: new string[6] { "adder", "banshee", "bullet", "carboniz", "cheetah", "coquette" },
                    isFirstPage: true,
                    isLastPage: false
                );
                GridSelector.AddPage
                (
                    names: new string[6] { "entityxf", "feltzer2", "hotknife", "ztype", "verlierer2", "turismor" },
                    descriptions: new string[6] { "", "", "", "", "", "" },
                    textureDicts: new string[6] { "lgm_default", "lgm_default", "lgm_default", "lgm_default", "lgm_dlc_apartments", "lgm_dlc_business" },
                    textureNames: new string[6] { "entityxf", "feltzer", "hotknife", "ztype", "verlier", "turismor" },
                    isFirstPage: false,
                    isLastPage: false
                );
                GridSelector.AddPage
                (
                    names: new string[6] { "zentorno", "brawler", "osiris", "t20", "ruston", "bifta" },
                    descriptions: new string[6] { "", "", "", "", "", "" },
                    textureDicts: new string[6] { "lgm_dlc_business2", "lgm_dlc_luxe", "lgm_dlc_luxe", "lgm_dlc_luxe", "lgm_dlc_specialraces", "sssa_default" },
                    textureNames: new string[6] { "zentorno", "brawler", "osiris", "t20", "ruston", "bifta" },
                    isFirstPage: false,
                    isLastPage: false
                );
                GridSelector.AddPage
                (
                    names: new string[6] { "dune", "bodhi2", "issi2", "kalahari", "rebel", "contender" },
                    descriptions: new string[6] { "", "", "", "", "", "" },
                    textureDicts: new string[6] { "sssa_default", "sssa_default", "sssa_default", "sssa_default", "sssa_default", "sssa_dlc_stunt" },
                    textureNames: new string[6] { "dune", "bodhi2", "issi2", "kalahari", "rebel", "contender" },
                    isFirstPage: false,
                    isLastPage: false
                );
                GridSelector.AddPage
                (
                    names: new string[6] { "rallytruck", "trophytruck2", "trophytruck", "kamacho", "marshall", "monster" },
                    descriptions: new string[6] { "", "", "", "", "", "" },
                    textureDicts: new string[6] { "sssa_dlc_stunt", "sssa_dlc_stunt", "sssa_dlc_stunt", "sssa_dlc_xmas2017", "candc_default", "candc_default" },
                    textureNames: new string[6] { "rallytruck", "trophy2", "trophy", "kamacho", "marshall", "monster" },
                    isFirstPage: false,
                    isLastPage: false
                );
                GridSelector.AddPage
                (
                    names: new string[6] { "voltic2", "wastelander", "vigilante", "caddy3", "comet4", "comet5" },
                    descriptions: new string[6] { "", "", "", "", "", "" },
                    textureDicts: new string[6] { "candc_importexport", "candc_importexport", "candc_smuggler", "foreclosures_bunker", "lgm_dlc_xmas2017", "lgm_dlc_xmas2017" },
                    textureNames: new string[6] { "voltic2", "wastlndr", "vigilante", "transportationb_2", "comet4", "comet5" },
                    isFirstPage: false,
                    isLastPage: true
                );
                //await GridSelector.LoadPage(0, 0, 0, GridSelector.GridPages.Count());

                for (var p = 0; p < GridSelector.GridPages.Count(); p++)
                {
                    await GridSelector.LoadPage(p, 0, 0, GridSelector.GridPages.Count());
                }
                await GridSelector.LoadPage(0, 0, 0, GridSelector.GridPages.Count());
                //GridSelector.currentPage = GridSelector.GridPages[0];
            }

            while (LimboActive)
            {
                await Delay(0);
                if (LimboActive && !IsPauseMenuActive() && !IsPauseMenuRestarting())
                {
                    //Game.DisableAllControlsThisFrame(0);
                    //Game.EnableControlThisFrame(0, Control.MpTextChatAll);
                    //Game.EnableControlThisFrame(0, Control.FrontendPause);
                    //Game.EnableControlThisFrame(0, Control.FrontendPauseAlternate);
                    Game.DisableControlThisFrame(0, Control.PhoneLeft);
                    Game.DisableControlThisFrame(0, Control.PhoneRight);
                    Game.DisableControlThisFrame(0, Control.PhoneUp);
                    Game.DisableControlThisFrame(0, Control.PhoneDown);
                    Game.DisableControlThisFrame(0, Control.PhoneSelect);
                    //Game.DisableControlThisFrame(0, Control.PhoneLeft);

                    if (Game.IsDisabledControlJustPressed(0, Control.PhoneLeft))
                    {
                        GridSelector.GoLeft();
                    }
                    if (Game.IsDisabledControlJustPressed(0, Control.PhoneRight))
                    {
                        GridSelector.GoRight();
                    }
                    if (Game.IsDisabledControlJustPressed(0, Control.PhoneUp))
                    {
                        GridSelector.GoUp();
                    }
                    if (Game.IsDisabledControlJustPressed(0, Control.PhoneDown))
                    {
                        GridSelector.GoDown();
                    }
                    if (Game.IsDisabledControlJustPressed(0, Control.PhoneSelect))
                    {
                        GridSelector.Select();
                        if (GridSelector.currentHoverIndex != 8 && GridSelector.currentHoverIndex != 6) // they did NOT press prev/next page.
                        {
                            if (GridSelector.currentHoverIndex == 7) // user confirmed their selection.
                            {
                                if (IsPedInAnyVehicle(PlayerPedId(), false))
                                {
                                    var veh = GetVehiclePedIsIn(PlayerPedId(), false);
                                    SetEntityAsMissionEntity(veh, false, false);
                                    DeleteVehicle(ref veh);
                                }
                                dynamic retval = new Spawns().GetSpawns();
                                var spawnPoint = retval[PlayerId()];
                                Vector3 sp = new Vector3(float.Parse(spawnPoint.x.ToString()), float.Parse(spawnPoint.y.ToString()), float.Parse(spawnPoint.z.ToString()));
                                float heading = 0f;
                                if (spawnPoint.heading.ToString().IndexOf(".") != -1)
                                {
                                    float.TryParse(spawnPoint.heading.ToString(), out heading);
                                }

                                await Utility.TransitionToCoords(new Vector4(sp.X, sp.Y, sp.Z, heading), (uint)GetHashKey(GridSelector.GetSelectedItemName()));
                                LimboActive = false;
                                SumoClient.currentPhase = SumoClient.GamePhase.STARTED;

                                TransitionFromBlurred(500f);
                            }
                        }
                    }
                }

                // Just checking again in case it got turned off mid execution in this thread.
                if (LimboActive)
                {
                    await GridSelector.ShowPage(GridSelector.currentPage != -1 ? GridSelector.currentPage : 0,
                    GridSelector.GridPages.Count() != 0 ? GridSelector.GridPages.Count() : 1);
                    DisplayRadar(false);
                }
                else
                {
                    break;
                }
            }
        }

        public enum MessageType
        {
            KNOCKED_OUT,
            ROUND_OVER,
            ROUND_NEW,
            WINNER,
            LOSER,
            CUSTOM
        };

        public static async Task ShowShard(MessageType type, string title, string subtitle, int backgroundColor, int textColor, int duration)
        {
            var scale = new Scaleform("MP_BIG_MESSAGE_FREEMODE");
            if (!HasNamedScaleformMovieLoaded("MP_BIG_MESSAGE_FREEMODE") || !scale.IsLoaded)
                RequestScaleformMovie("MP_BIG_MESSAGE_FREEMODE");
            while (!HasNamedScaleformMovieLoaded("MP_BIG_MESSAGE_FREEMODE") || !scale.IsLoaded)
            {
                await Delay(0);
            }
            switch (type)
            {
                case MessageType.KNOCKED_OUT:
                    var timer = GetGameTimer();
                    scale.CallFunction("SHOW_SHARD_WASTED_MP_MESSAGE", title, subtitle, 0, true, true);
                    StartScreenEffect("MinigameEndNeutral", duration, false);
                    //SumoSound.Play(SumoSound.sound.PLAYER_DIED);
                    SumoSound.Play(SumoSound.sound.SHARD_RESULT);
                    while (GetGameTimer() - timer < duration)
                    {
                        scale.Render2D();
                        await Delay(0);
                    }
                    StopAllScreenEffects();


                    break;
                default:
                    break;
            }


        }

    }
}
