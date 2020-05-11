import pypdn
import matplotlib.pyplot as plt
from PIL import Image
import numpy
import sys

# Script for turning a .pdn layered image into a correctly spaced/sized tile image
# Eg. turns TurretTile.pdn into TurretTile.png
# The pdn should have the base layer at the bottom with each permutation from bottom to top, which will be exported as left to right in the tilemap
#

if (len(sys.argv) < 2):
    print("Not enough arguments")
    exit()

print("Loading image: ", sys.argv[1])
layeredImage = pypdn.read(sys.argv[1])
print("Loaded")

layer = layeredImage.layers[0]
layer.visible = True
layer.opacity = 255
layer.blendMode = pypdn.BlendType.Normal

twidth = layeredImage.width + int(layeredImage.width/8)
theight = layeredImage.height + int(layeredImage.height/8)

img = Image.new('RGBA', (twidth * (len(layeredImage.layers) - 1), theight), color = '#00000000')

for i in range(1, len(layeredImage.layers)):
    for j in range(1, len(layeredImage.layers)):
        layeredImage.layers[j].visible = False
        
    layeredImage.layers[i].visible = True
    flatImage = layeredImage.flatten(asByte=True)
    lay = Image.new('RGB', (layeredImage.width, layeredImage.height), color = '#00000000')
    lay = Image.fromarray(flatImage)
    
    for x in range(0, int(layeredImage.width/8)):
        for y in range(0, int(layeredImage.height/8)):
            crp = lay.crop((x * 8, y * 8, x * 8 + 8, y * 8 + 8))
            img.paste(crp, (twidth * (i-1) + x * 9, y * 9))

img = img.resize((img.width * 2, img.height * 2), Image.NEAREST)
img.save(sys.argv[1][:-4] + ".png")
#plt.figure()
#plt.imshow(numpy.asarray(img))
#plt.show()
