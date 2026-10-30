"""Read-only inventory check for the delivered Unity archive."""
import json
import tarfile
from pathlib import Path

root = Path(__file__).resolve().parents[2]
archive = root / 'Exports/WorldOfThePack-CardUI-v01.unitypackage'
with tarfile.open(archive, 'r:gz') as package:
    paths = [package.extractfile(item).read().decode('utf-8').strip() for item in package.getmembers() if item.name.endswith('/pathname')]
prefix = 'Assets/UI/WorldOfThePack/'
heroes = ['HeroWolvy', 'PrincessElva', 'FairyLulu', 'CaptainBobo', 'ScoutPip', 'WizardMomo', 'TinkerTilly']
required = [prefix + 'Prefabs/CardPortrait.prefab', prefix + 'Prefabs/HeroCardHand.prefab', prefix + 'Prefabs/PortraitCardCanvas.prefab', prefix + 'Scenes/CardPortraitShowcase.unity', prefix + 'Fonts/LilitaOne-Regular.ttf', prefix + 'Fonts/OFL.txt']
required += [prefix + f'Prefabs/{hero}_CardPortrait.prefab' for hero in heroes]
required += [prefix + f'Art/Portraits/{hero}_{form}.png' for hero in heroes for form in ['Human', 'Wolf']]
missing = [path for path in required if path not in paths]
report = {'archive': str(archive), 'bytes': archive.stat().st_size, 'assetPaths': len(paths), 'requiredAssets': len(required), 'missing': missing, 'verified': not missing}
(root / 'Design/CardUI/QA/package-audit.json').write_text(json.dumps(report, indent=2), encoding='utf-8')
print(json.dumps(report, indent=2))
if missing:
    raise SystemExit(1)
