# Character production record

The seven-human/seven-wolf sprite package is stored under `Assets/Characters`. Built-in image generation produced the source art and the targeted corrections; mechanical extraction and sheet packing standardized the frame canvases.

`generation-manifest.json` records source paths, character identities, form, exact prompts, and replacement/correction history. Selected raw PNGs are copied into `Sources/<Character>/<Form>-source.png`. Rejected source generations are not imported into Unity.

`Normalize-Sheets.ps1` validates real alpha, identifies isolated character silhouettes, recovers pieces crossing nominal grid boundaries where separated, excludes disconnected neighboring fragments, applies one scale per form based on its idle height, aligns each row's median ground position, and packs 256px cells. Connected neighboring poses are partitioned at nominal cell boundaries. It does not redraw anatomy or invent missing pose content.

The layout audit records source-boundary cases rather than hiding them. Zero normalized clipping means no additional clipping was introduced by packing; it cannot prove the original generated artwork had no construction errors. Source effects that reach their original tile edge still need visual review. The normalized effects have additional transparent padding for sampling.

Human and wolf forms share an approximately 160px idle silhouette height and identical pivots. The camera direction is north/rear-facing. One sheet per form contains six frames per animation, with four human rows and six wolf rows. Each character also has three six-frame transparent effect sequences.

Re-run normalization with PowerShell 7 on Windows, then build in the project's Unity 6000.3.13f1 editor. The normalizer's assembly references target the available PowerShell/.NET runtime on this machine. Unity import and verification use the installed 2D Sprite Editor data-provider API, not hand-authored importer YAML.

QA output is in `QA`; the editor log is `unity-build.log`. Assets are distinct from the earlier presentation mockups in `Assets/Visuals`.
