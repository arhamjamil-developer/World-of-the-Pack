"""Read-only inventory and required-file verification of the exported Unity archive."""
import json
import tarfile
from pathlib import Path

root = Path(__file__).resolve().parents[2]
archive = root / 'Exports/WorldOfThePack-CombatVFX-v01.unitypackage'
prefix = 'Assets/VFX/WorldOfThePack/'
items = json.loads((root / (prefix + 'Data/sequence-manifest.json')).read_text(encoding='utf-8-sig'))
required = [prefix + p for p in ['README.md', 'Scenes/CombatVFXShowcase.unity', 'Shaders/SpriteFeedback.shader', 'Art/CombatVFX.spriteatlas']]
required += [prefix + f'Materials/{name}.mat' for name in ['SoftAlpha', 'SoftAdditive', 'GroundWarning', 'CharacterFeedback']]
for item in items:
    stem = item['group'] + '/' + item['name']
    required += [prefix + 'Data/' + stem + '.asset', prefix + 'Prefabs/' + stem + '.prefab']
    required += [prefix + f'Art/{stem}_{frame:02}.png' for frame in range(item['frames'])]
    if item['frames'] > 1:
        required += [prefix + 'Animations/' + stem + '.anim']
required += ['Assets/UI/WorldOfThePack/Fonts/OFL.txt', 'Assets/UI/WorldOfThePack/Fonts/LilitaOne-Regular.ttf']
with tarfile.open(archive, 'r:gz') as package:
    paths = [package.extractfile(item).read().decode('utf-8').strip() for item in package.getmembers() if item.name.endswith('/pathname')]
missing = sorted(set(required) - set(paths))
report = {'archive': str(archive), 'bytes': archive.stat().st_size, 'assetPaths': len(paths), 'requiredAssets': len(required), 'missing': missing, 'verified': not missing}
(root / 'Design/CombatVFX/QA/package-audit.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(report, indent=2))
if missing:
    raise SystemExit(1)
