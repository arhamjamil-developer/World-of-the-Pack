import os
import cv2
import numpy as np
from PIL import Image, ImageDraw, ImageFilter

MOCKUP_PATH = r'C:/Users/ASUS/.gemini/antigravity/brain/0e5eed53-13e3-471a-9446-c17ee0f35ffa/.user_uploaded/media_1793398943220_26011c8e.jpg'
UI_CONTROLS_DIR = r'B:\World of the Pack\Assets\UI\WorldOfThePack\Art\Controls'
UI_FRAMES_DIR = r'B:\World of the Pack\Assets\UI\WorldOfThePack\Art\Frames'
VISUALS_DIR = r'B:\World of the Pack\Assets\Visuals'
ENEMIES_DIR = r'B:\World of the Pack\Assets\Characters\Enemies'
VFX_DIR = r'B:\World of the Pack\Assets\VFX\WorldOfThePack\Art'

os.makedirs(UI_CONTROLS_DIR, exist_ok=True)
os.makedirs(UI_FRAMES_DIR, exist_ok=True)
os.makedirs(VISUALS_DIR, exist_ok=True)
os.makedirs(ENEMIES_DIR, exist_ok=True)
os.makedirs(VFX_DIR, exist_ok=True)

img_pil = Image.open(MOCKUP_PATH).convert('RGBA')
print("Mockup loaded:", img_pil.size)

# Helper function to remove background around a subject using color difference from sampled background color
def remove_bg(crop_img, bg_color_rgb, tolerance=35):
    arr = np.array(crop_img)
    rgb = arr[:, :, :3]
    diff = np.sqrt(np.sum((rgb.astype(np.float32) - np.array(bg_color_rgb, dtype=np.float32)) ** 2, axis=2))
    alpha = np.clip((diff - tolerance) * (255.0 / tolerance), 0, 255).astype(np.uint8)
    arr[:, :, 3] = np.minimum(arr[:, :, 3], alpha)
    return Image.fromarray(arr)

# 1. WAVE BADGE (x: 10, y: 15, x2: 145, y2: 68)
wave_img = img_pil.crop((10, 15, 145, 68))
wave_img.save(os.path.join(UI_CONTROLS_DIR, 'WaveBadge_Panel.png'))

# 2. TITLE TREATMENT "WORLD OF THE PACK"
title_img = img_pil.crop((175, 4, 400, 64))
title_img.save(os.path.join(UI_CONTROLS_DIR, 'Title_WorldOfThePack.png'))

# 3. PACK HEARTS
hearts_strip = img_pil.crop((215, 61, 360, 85))
hearts_strip.save(os.path.join(UI_CONTROLS_DIR, 'PackHearts_Strip.png'))

# Single full heart
heart_full = img_pil.crop((222, 63, 246, 85))
heart_full.save(os.path.join(UI_CONTROLS_DIR, 'Heart_Full.png'))

# Single empty heart
heart_empty = img_pil.crop((326, 63, 350, 85))
heart_empty.save(os.path.join(UI_CONTROLS_DIR, 'Heart_Empty.png'))

# 4. PAUSE BUTTON
pause_img = img_pil.crop((505, 10, 565, 65))
pause_img.save(os.path.join(UI_CONTROLS_DIR, 'PauseButton_Round.png'))

# 5. NEXT CAMP PANEL
camp_panel = img_pil.crop((398, 62, 570, 105))
camp_panel.save(os.path.join(UI_CONTROLS_DIR, 'NextCamp_Panel.png'))

# Tent icon
tent_icon = img_pil.crop((404, 63, 442, 100))
tent_icon.save(os.path.join(UI_CONTROLS_DIR, 'Camp_TentIcon.png'))

# 6. BOSS HUD (BRAMBLE BRUTE)
boss_bar_full = img_pil.crop((190, 88, 385, 125))
boss_bar_full.save(os.path.join(UI_CONTROLS_DIR, 'BossBar_Full.png'))

# Acorn icon
acorn_icon = img_pil.crop((192, 90, 224, 124))
acorn_icon.save(os.path.join(UI_CONTROLS_DIR, 'Boss_AcornIcon.png'))

# 7. LOWER CONTROLS
form_btn = img_pil.crop((12, 720, 180, 768))
form_btn.save(os.path.join(UI_CONTROLS_DIR, 'FormationButton_Wood.png'))

moon_btn = img_pil.crop((390, 720, 565, 768))
moon_btn.save(os.path.join(UI_CONTROLS_DIR, 'MoonshiftButton_Wood.png'))

dots_img = img_pil.crop((220, 735, 355, 760))
dots_img.save(os.path.join(UI_CONTROLS_DIR, 'EnergyDots_Bar.png'))

# 8. ROLE BADGES
badge_paw = img_pil.crop((41, 773, 85, 817))
badge_paw.save(os.path.join(UI_CONTROLS_DIR, 'RoleBadge_Paw.png'))

badge_leaf = img_pil.crop((209, 773, 253, 817))
badge_leaf.save(os.path.join(UI_CONTROLS_DIR, 'RoleBadge_Leaf.png'))

badge_moon = img_pil.crop((392, 773, 436, 817))
badge_moon.save(os.path.join(UI_CONTROLS_DIR, 'RoleBadge_Moon.png'))

# 9. BUTTONS
deploy_btn = img_pil.crop((38, 918, 150, 966))
deploy_btn.save(os.path.join(UI_CONTROLS_DIR, 'DeployButton_Green.png'))

charging_btn = img_pil.crop((211, 918, 323, 966))
charging_btn.save(os.path.join(UI_CONTROLS_DIR, 'ChargingButton_Navy.png'))

reroll_btn = img_pil.crop((153, 920, 187, 964))
reroll_btn.save(os.path.join(UI_CONTROLS_DIR, 'RerollButton_Circle.png'))

# 10. DEPLOY RING & INDICATOR
deploy_ring = img_pil.crop((235, 630, 340, 725))
deploy_ring.save(os.path.join(UI_CONTROLS_DIR, 'DeployIndicator_Raw.png'))

deploy_pill = img_pil.crop((240, 692, 336, 723))
deploy_pill.save(os.path.join(UI_CONTROLS_DIR, 'DeployHere_Pill.png'))

chevron_arrows = img_pil.crop((273, 574, 298, 608))
chevron_arrows.save(os.path.join(UI_CONTROLS_DIR, 'Chevron_UpArrows.png'))

# 11. GROUND RINGS
blue_ring = img_pil.crop((396, 615, 470, 660))
blue_ring.save(os.path.join(VFX_DIR, 'GroundRing_AllyBlue.png'))

orange_ring = img_pil.crop((372, 275, 442, 316))
orange_ring.save(os.path.join(VFX_DIR, 'GroundRing_EnemyOrange.png'))

danger_circle = img_pil.crop((230, 240, 345, 305))
danger_circle.save(os.path.join(VFX_DIR, 'Telegraph_OrangeSlam1_2s.png'))

# 12. COMBAT VFX
slash_vfx = img_pil.crop((165, 275, 285, 430))
slash_vfx.save(os.path.join(VFX_DIR, 'VFX_GoldenMoonSlash.png'))

shield_bubble = img_pil.crop((295, 335, 390, 435))
shield_bubble.save(os.path.join(VFX_DIR, 'VFX_AcornShieldBubble.png'))

momo_circle = img_pil.crop((235, 500, 345, 555))
momo_circle.save(os.path.join(VFX_DIR, 'VFX_MomoStarAura.png'))

lulu_magic = img_pil.crop((175, 545, 255, 610))
lulu_magic.save(os.path.join(VFX_DIR, 'VFX_LuluMoonSparkles.png'))

pip_ghost = img_pil.crop((460, 435, 515, 495))
pip_ghost.save(os.path.join(VFX_DIR, 'VFX_PipGhostAfterimage.png'))

tilly_turret = img_pil.crop((325, 580, 395, 645))
tilly_turret.save(os.path.join(VFX_DIR, 'Prop_TinkerTurret.png'))

# 13. CAMPSITE & POND PROPS
campsite_crop = img_pil.crop((440, 175, 575, 340))
campsite_crop.save(os.path.join(VISUALS_DIR, 'SunnyForest_CampsiteProp.png'))

pond_crop = img_pil.crop((0, 320, 115, 510))
pond_crop.save(os.path.join(VISUALS_DIR, 'SunnyForest_WaterPondProp.png'))

# 14. EXTRACT ENEMIES WITH TRANSPARENCY
# Bramble Brute: (205, 130, 375, 255)
# Soldier: (370, 220, 445, 305)
# Archer: (140, 125, 205, 195)
# Priest: (370, 130, 445, 198)

def cut_with_mask(crop_box, out_name):
    crop = img_pil.crop(crop_box)
    arr = np.array(crop)
    # Background in the road is golden dirt: roughly RGB (235, 175, 110)
    # Let's perform precise alpha matting using GrabCut
    img_bgr = cv2.cvtColor(arr[:, :, :3], cv2.COLOR_RGB2BGR)
    mask = np.zeros(img_bgr.shape[:2], np.uint8)
    bgdModel = np.zeros((1,65), np.float64)
    fgdModel = np.zeros((1,65), np.float64)
    h_c, w_c = img_bgr.shape[:2]
    rect = (3, 3, w_c - 6, h_c - 6)
    cv2.grabCut(img_bgr, mask, rect, bgdModel, fgdModel, 5, cv2.GC_INIT_WITH_RECT)
    mask2 = np.where((mask==2)|(mask==0), 0, 1).astype('uint8')
    arr[:, :, 3] = mask2 * 255
    res = Image.fromarray(arr)
    res.save(os.path.join(ENEMIES_DIR, out_name))
    return res

cut_with_mask((205, 130, 375, 255), 'Boss_BrambleBrute_South.png')
cut_with_mask((370, 220, 445, 305), 'Enemy_Soldier_South.png')
cut_with_mask((140, 125, 205, 195), 'Enemy_Archer_South.png')
cut_with_mask((370, 130, 445, 198), 'Enemy_Priest_South.png')

# 15. Create a Contact Sheet of Extracted Assets
assets_to_sheet = [
    ('WaveBadge', wave_img),
    ('Title', title_img),
    ('Hearts', hearts_strip),
    ('BossBar', boss_bar_full),
    ('NextCamp', camp_panel),
    ('Formation', form_btn),
    ('Moonshift', moon_btn),
    ('DeployBtn', deploy_btn),
    ('ChargingBtn', charging_btn),
    ('Boss', Image.open(os.path.join(ENEMIES_DIR, 'Boss_BrambleBrute_South.png'))),
    ('Soldier', Image.open(os.path.join(ENEMIES_DIR, 'Enemy_Soldier_South.png'))),
    ('Archer', Image.open(os.path.join(ENEMIES_DIR, 'Enemy_Archer_South.png'))),
    ('Priest', Image.open(os.path.join(ENEMIES_DIR, 'Enemy_Priest_South.png'))),
]

sheet = Image.new('RGBA', (1024, 768), (30, 35, 45, 255))
draw = ImageDraw.Draw(sheet)
cur_x, cur_y = 20, 20
row_max_h = 0

for label, aimg in assets_to_sheet:
    aimg_rgba = aimg.convert('RGBA')
    w, h = aimg_rgba.size
    if cur_x + w > 1000:
        cur_x = 20
        cur_y += row_max_h + 30
        row_max_h = 0
    sheet.paste(aimg_rgba, (cur_x, cur_y), aimg_rgba)
    draw.text((cur_x, cur_y + h + 4), label, fill=(255, 255, 200, 255))
    cur_x += w + 25
    row_max_h = max(row_max_h, h)

sheet.save(os.path.join(VISUALS_DIR, 'ContactSheet_ExtractedMockupAssets.png'))
print("Successfully generated all assets and contact sheet!")
