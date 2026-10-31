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

# 1. Precise Alpha Extraction for Enemies
def extract_enemy(box, out_name, bg_color_rgb=(230, 172, 107), tol=40):
    crop = img_pil.crop(box)
    arr = np.array(crop).astype(np.float32)
    rgb = arr[:, :, :3]
    diff = np.sqrt(np.sum((rgb - np.array(bg_color_rgb, dtype=np.float32)) ** 2, axis=2))
    
    # GrabCut on the cropped area
    bgr = cv2.cvtColor(np.array(crop)[:, :, :3], cv2.COLOR_RGB2BGR)
    mask = np.zeros(bgr.shape[:2], np.uint8)
    bgd = np.zeros((1, 65), np.float64)
    fgd = np.zeros((1, 65), np.float64)
    h, w = bgr.shape[:2]
    rect = (2, 2, w - 4, h - 4)
    try:
        cv2.grabCut(bgr, mask, rect, bgd, fgd, 6, cv2.GC_INIT_WITH_RECT)
        fg_mask = np.where((mask == 1) | (mask == 3), 255, 0).astype(np.uint8)
    except:
        fg_mask = np.where(diff > tol, 255, 0).astype(np.uint8)
        
    # Refine mask edges
    fg_mask = cv2.GaussianBlur(fg_mask, (3, 3), 0)
    out_arr = np.array(crop)
    out_arr[:, :, 3] = fg_mask
    res = Image.fromarray(out_arr)
    res.save(os.path.join(ENEMIES_DIR, out_name))
    print(f"Saved enemy {out_name} with size {res.size}")

# Bramble Brute:
extract_enemy((205, 130, 375, 255), 'Boss_BrambleBrute_South.png')
# Soldier:
extract_enemy((370, 222, 442, 302), 'Enemy_Soldier_South.png')
# Archer:
extract_enemy((145, 140, 215, 202), 'Enemy_Archer_South.png')
# Priest:
extract_enemy((372, 136, 442, 198), 'Enemy_Priest_South.png')

# 2. Clean Nine-Sliceable Wooden Frame for Cards
# Create a polished HD Golden Card Frame (256 x 340) with wooden back and rounded gold rim
frame_w, frame_h = 256, 340
card_frame = Image.new('RGBA', (frame_w, frame_h), (0, 0, 0, 0))
draw = ImageDraw.Draw(card_frame)

# Wooden background
draw.rounded_rectangle([6, 6, frame_w - 7, frame_h - 7], radius=24, fill=(48, 28, 16, 255))
# Inner cream parchment
draw.rounded_rectangle([12, 12, frame_w - 13, frame_h - 13], radius=18, fill=(245, 228, 196, 255))
# Golden outer border
draw.rounded_rectangle([4, 4, frame_w - 5, frame_h - 5], radius=26, outline=(245, 198, 48, 255), width=5)
# Golden inner highlight
draw.rounded_rectangle([8, 8, frame_w - 9, frame_h - 9], radius=22, outline=(255, 235, 120, 255), width=2)
# Dark outer stroke
draw.rounded_rectangle([2, 2, frame_w - 3, frame_h - 3], radius=28, outline=(20, 10, 5, 255), width=2)
card_frame.save(os.path.join(UI_FRAMES_DIR, 'CardFrame_HD.png'))

# 3. Clean Nine-Sliceable Action Buttons
# Deploy Button: Bright Green rounded button
bw, bh = 240, 72
deploy_btn = Image.new('RGBA', (bw, bh), (0, 0, 0, 0))
d_draw = ImageDraw.Draw(deploy_btn)
d_draw.rounded_rectangle([4, 4, bw - 5, bh - 5], radius=20, fill=(72, 196, 24, 255))
# highlight gradient on top
d_draw.rounded_rectangle([6, 6, bw - 7, bh // 2], radius=16, fill=(136, 235, 54, 255))
d_draw.rounded_rectangle([4, 4, bw - 5, bh - 5], radius=20, outline=(255, 255, 255, 180), width=3)
d_draw.rounded_rectangle([2, 2, bw - 3, bh - 3], radius=22, outline=(28, 90, 8, 255), width=2)
deploy_btn.save(os.path.join(UI_CONTROLS_DIR, 'DeployButton_HD.png'))

# Charging Button: Navy blue rounded button
charging_btn = Image.new('RGBA', (bw, bh), (0, 0, 0, 0))
c_draw = ImageDraw.Draw(charging_btn)
c_draw.rounded_rectangle([4, 4, bw - 5, bh - 5], radius=20, fill=(38, 76, 108, 255))
c_draw.rounded_rectangle([6, 6, bw - 7, bh // 2], radius=16, fill=(58, 110, 152, 255))
c_draw.rounded_rectangle([4, 4, bw - 5, bh - 5], radius=20, outline=(140, 195, 235, 140), width=2)
c_draw.rounded_rectangle([2, 2, bw - 3, bh - 3], radius=22, outline=(14, 32, 48, 255), width=2)
charging_btn.save(os.path.join(UI_CONTROLS_DIR, 'ChargingButton_HD.png'))

# 4. Energy Bar Track & Fill
track_w, track_h = 240, 36
track_img = Image.new('RGBA', (track_w, track_h), (0, 0, 0, 0))
t_draw = ImageDraw.Draw(track_img)
t_draw.rounded_rectangle([2, 2, track_w - 3, track_h - 3], radius=16, fill=(35, 22, 14, 255), outline=(18, 10, 6, 255), width=2)
track_img.save(os.path.join(UI_CONTROLS_DIR, 'EnergyTrack_HD.png'))

fill_green = Image.new('RGBA', (track_w, track_h), (0, 0, 0, 0))
fg_draw = ImageDraw.Draw(fill_green)
fg_draw.rounded_rectangle([4, 4, track_w - 5, track_h - 5], radius=14, fill=(116, 228, 38, 255))
fg_draw.rounded_rectangle([6, 6, track_w - 7, track_h // 2], radius=10, fill=(188, 255, 110, 255))
fill_green.save(os.path.join(UI_CONTROLS_DIR, 'EnergyFill_Green_HD.png'))

fill_yellow = Image.new('RGBA', (track_w, track_h), (0, 0, 0, 0))
fy_draw = ImageDraw.Draw(fill_yellow)
fy_draw.rounded_rectangle([4, 4, track_w - 5, track_h - 5], radius=14, fill=(245, 188, 32, 255))
fy_draw.rounded_rectangle([6, 6, track_w - 7, track_h // 2], radius=10, fill=(255, 230, 110, 255))
fill_yellow.save(os.path.join(UI_CONTROLS_DIR, 'EnergyFill_Yellow_HD.png'))

# 5. Clean Reroll / Refresh Button (48 x 48)
rw = 64
reroll_img = Image.new('RGBA', (rw, rw), (0, 0, 0, 0))
r_draw = ImageDraw.Draw(reroll_img)
r_draw.ellipse([4, 4, rw - 5, rw - 5], fill=(24, 60, 78, 255), outline=(160, 220, 255, 255), width=3)
# Draw circular arrow
r_draw.arc([16, 16, rw - 17, rw - 17], start=30, end=300, fill=(230, 245, 255, 255), width=4)
reroll_img.save(os.path.join(UI_CONTROLS_DIR, 'RerollButton_HD.png'))

# 6. Blue Allied Ground Ring (128 x 64 perspective ellipse)
ring_w, ring_h = 160, 80
ally_ring = Image.new('RGBA', (ring_w, ring_h), (0, 0, 0, 0))
ar_draw = ImageDraw.Draw(ally_ring)
ar_draw.ellipse([6, 6, ring_w - 7, ring_h - 7], outline=(60, 180, 255, 240), width=6)
ar_draw.ellipse([10, 10, ring_w - 11, ring_h - 11], outline=(180, 235, 255, 255), width=2)
ally_ring = ally_ring.filter(ImageFilter.GaussianBlur(1.0))
ally_ring.save(os.path.join(VFX_DIR, 'GroundRing_AllyBlue_HD.png'))

# 7. Orange Enemy Ground Ring
enemy_ring = Image.new('RGBA', (ring_w, ring_h), (0, 0, 0, 0))
er_draw = ImageDraw.Draw(enemy_ring)
er_draw.ellipse([6, 6, ring_w - 7, ring_h - 7], outline=(255, 110, 30, 240), width=6)
er_draw.ellipse([10, 10, ring_w - 11, ring_h - 11], outline=(255, 210, 120, 255), width=2)
enemy_ring = enemy_ring.filter(ImageFilter.GaussianBlur(1.0))
enemy_ring.save(os.path.join(VFX_DIR, 'GroundRing_EnemyOrange_HD.png'))

# 8. Deploy Indicator Ring with Chevrons and "DEPLOY HERE"
deploy_ind = Image.new('RGBA', (200, 140), (0, 0, 0, 0))
di_draw = ImageDraw.Draw(deploy_ind)
# Hologram base ellipse
di_draw.ellipse([10, 25, 190, 105], outline=(80, 195, 255, 255), width=5)
di_draw.ellipse([18, 31, 182, 99], outline=(180, 240, 255, 200), width=2)
# Deploy Here Pill badge at bottom
di_draw.rounded_rectangle([35, 88, 165, 128], radius=18, fill=(40, 140, 230, 255), outline=(180, 235, 255, 255), width=3)
deploy_ind.save(os.path.join(UI_CONTROLS_DIR, 'DeployIndicator_Hologram_HD.png'))

print("Generated clean HD UI sprites!")
