import blockTextureAtlas from "./blocks/Blocks.webp";
import Cactus from "./blocks/models/Cactus.gltf";
import CraftTable from "./blocks/models/CraftTable.gltf";
import HumanMale from "./player/models/HumanMale.gltf";
import HumanMaleTexture1 from "./player/HumanMale1.webp";

const Assets = {
	blocks: {
		models: {
			Cactus,
			CraftTable,
		},
		atlas: blockTextureAtlas,
	},
	player: {
		models: {
			HumanMale,
		},
		textures: {
			HumanMaleTexture1,
		},
	},
};
export default Assets;
