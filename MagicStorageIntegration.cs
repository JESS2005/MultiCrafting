using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MagicStorage;
using System.Linq;
using MagicStorage.Components;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace MultiCrafting
{

	[JITWhenModsEnabled("MagicStorage")]
	static class MagicStorageIntegration
	{
		static Mod MagicStorage;

		//Should only check on load (?)
        public static bool Enabled => ModLoader.HasMod("MagicStorage");
        public static void Load()
		{
			ModLoader.TryGetMod("MagicStorage", out MagicStorage);
			if (Enabled)
				Initialize();
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void Initialize()
		{

		}

		public static void Unload()
		{
			if (Enabled)
				Unload_Inner();
			MagicStorage = null;
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		private static void Unload_Inner()
		{

		}

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static List<Item> StorageItems()
		{
			//MagicStorage= ModLoader.GetMod("MagicStorage");
            List<Item> stored = new List<Item>();

            if (StoragePlayer.LocalPlayer.GetStorageHeart() != null) { stored = StoragePlayer.LocalPlayer.GetStorageHeart().GetStoredItems()?.ToList() ?? new List<Item>(); }

			return stored;
		}

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void RefreshStorageUI()
        {
			StorageGUI.RefreshUI = true;
        }

		internal static int recCrafingDepth() {
			return ModContent.GetInstance<MagicStorageConfig>().recursionCraftingDepth;
        }
    }
}
