using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;

namespace CoinHP.API.DirectDetours{
	public static class Manager{
		private static readonly List<Hook> detours = new List<Hook>();
		private static readonly List<(MethodInfo, Delegate)> delegates = new List<(MethodInfo, Delegate)>();

		private static readonly Dictionary<string, MethodInfo> cachedMethods = new Dictionary<string, MethodInfo>();

		public static void Load(){
			try{
				MonoModHooks.RequestNativeAccess();

				//Usage: Forces hover text for the hearts UI to not display
				DetourHook(typeof(Main).GetCachedMethod("GUIBarsMouseOverLife"),
					typeof(Detours.Vanilla).GetCachedMethod(nameof(Detours.Vanilla.Main_GUIBarsMouseOverLife)));

				//Usage: preventing hearts from drawing in the settings menu
				//   For some fucking reason, the settings menu draws the resources differently than the inventory UI
				DetourHook(typeof(Main).GetCachedMethod("DrawInterface_Resources_Life"),
					typeof(Detours.Vanilla).GetCachedMethod(nameof(Detours.Vanilla.Main_DrawInterface_Resource_Life)));

				//Usage: forcing the "waitingForWorldEnter" bool to be true, even if the player doesn't have data for CoinPlayer
				DetourHook(typeof(Mod).Assembly.GetType("Terraria.ModLoader.IO.PlayerIO", throwOnError: true).GetCachedMethod("LoadModData"),
					typeof(Detours.TML).GetCachedMethod(nameof(Detours.TML.PlayerIO_LoadModData)));
			}catch(Exception ex){
				throw new Exception("An error occurred while doing patching in CoinHP." +
					"\nReport this error to the mod devs and disable the mod in the meantime." +
					"\n\n\n" + ex.ToString());
			}
		}

		private static MethodInfo GetCachedMethod(this Type type, string method){
			string key = $"{type.FullName}::{method}";
			if(cachedMethods.TryGetValue(key, out MethodInfo value))
				return value;

			return cachedMethods[key] = type.GetMethod(method, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
		}

		public static void Unload(){
			foreach(var hook in detours)
				hook.Undo();

			foreach((MethodInfo method, Delegate hook) in delegates)
				HookEndpointManager.Unmodify(method, hook);
		}

		private static void IntermediateLanguageHook(MethodInfo orig, MethodInfo modify){
			Delegate hook = Delegate.CreateDelegate(typeof(ILContext.Manipulator), modify);
			delegates.Add((orig, hook));
			HookEndpointManager.Modify(orig, hook);
		}

		private static void DetourHook(MethodInfo orig, MethodInfo modify){
			Hook hook = new Hook(orig, modify);
			detours.Add(hook);
			hook.Apply();
		}
	}
}
