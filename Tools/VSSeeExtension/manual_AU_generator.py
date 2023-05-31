'''
Script for generating Manual AUs which can be visualized in some softwares in real time
'''
import matplotlib.pyplot as plt
from matplotlib.widgets import Slider, Button
import sys
sys.path.append(sys.path[0]+'\\modules')
#sys.path.append('C:\\Users\\Abhi\\Desktop\\acads\\5-2_Thesis\\repos\\Thesis_repos\\Bachelor_Thesis\\Scripts\\modules')
print(sys.path)
from modules.gui.controller import Controller
import numpy as np
import matplotlib as mpl
mpl.rcParams['toolbar'] = 'None' 





#Adapted from modules/gui/gui.ipynb

def update_AUs(sliders,multiplier_sliders):
    '''
    Method for updating AUs to be sent once the slider data is changed
    '''
    # AU sliders
    au_descriptions = {
        "AU01_r": "Inner brow raiser",
        "AU02_r": "Outer brow raiser",
        "AU04_r": "Brow lowerer",
        "AU05_r": "Upper lid raiser",
        "AU06_r": "Cheek raiser",
        "AU07_r": "Lid tightener",
        "AU09_r": "Nose wrinkler",
        "AU10_r": "Upper lip raiser",
        "AU12_r": "Lip corner puller",
        "AU14_r": "Dimpler",
        "AU15_r": "Lip corner depressor",
        "AU17_r": "Chin raiser",
        "AU20_r": "Lip stretcher",
        "AU23_r": "Lip tightener",
        "AU25_r": "Lips part",
        "AU26_r": "Jaw drop",
        "AU45_r": "Blink",
    #     "AU61": "Eyes left",
    #     "AU62": "Eyes right",
    #     "AU63": "Eyes up",
    #     "AU64": "Eyes down"
    }

    dict_config = {}
    for slider,multiplier_slider in zip(sliders,multiplier_sliders):
        dict_config[slider.label.get_text().split('_')[0]] = slider.val * multiplier_slider.val

    for pose in ["pose_Rx", "pose_Ry", "pose_Rz"]:
        dict_config[pose] = 0

    return dict_config


def create_sliders(fig,label,i,multiplier=1):
    '''
    Method for creating sliders to be displayed in windows
    '''
    # Make a horizontal slider 
    h = 0.05*i
    axAU = fig.add_axes([0.25, h, 0.7, 0.01])
    axAU.margins(10)
    AU_slider = Slider(
    ax=axAU,
    label=label,
    valmin=0,
    valmax=1*multiplier,
    valinit=0 if multiplier==1 else 1 ,
    )
    return AU_slider



#adapted
controller = Controller(pub_ip="127.0.0.1", pub_port=5570, pub_key="gui.face_config", pub_bind=False,
                        deal_ip="127.0.0.1", deal_port=5580, deal_key="gui", deal_topic="multiplier",
                        deal2_ip="127.0.0.1", deal2_port=5581, deal2_key="gui", deal2_topic="dnn",
                        deal3_ip="127.0.0.1", deal3_port=5582, deal3_key="gui", deal3_topic="dnn"
                       )

labels = ['AU01_Inner brow raiser', 
          'AU02_Outer brow raiser',
          'AU04_Brow lowerer', 
          'AU05_Upper lid raiser', 
          'AU06_Cheek raiser', 
          'AU07_Lid tightener', 
          'AU09_Nose wrinkler', 
          'AU10_Upper lip raiser', 
          'AU12_Lip corner puller',
          'AU14_Dimpler', 
          'AU15_Lip corner depressor', 
          'AU17_Chin raiser', 
          'AU20_Lip stretcher', 
          'AU23_Lip tightener', 
          'AU25_Lips part', 
          'AU26_Jaw drop']
# Define initial parameters
init_AUs =[ 1 for i in range(len(labels)) ] 

# Create the figure  that we will manipulate
fig1 = plt.figure("Intensity",figsize=(8,3))
fig2 = plt.figure("Multiplier",figsize=(8,3))

#position the windows
fig1.canvas.manager.window.wm_geometry("+0+0")
fig2.canvas.manager.window.wm_geometry("+0+500")

#generate the sliders
sliders = []
# Make a horizontal slider to control the Intensity.
for i,label in enumerate(labels):
    sliders.append(create_sliders(fig1,label,i))

multiplier_sliders = []
for i,label in enumerate(labels):
    multiplier_sliders.append(create_sliders(fig2,label,i,10))

# The function to be called anytime a slider's value changes
def update(val):
    
    AUs = update_AUs(sliders,multiplier_sliders)
    controller.face_configuration(AUs)
    print(f'sent msg:{AUs} ')


# register the update function with each slider
sliders[0].on_changed(update)
sliders[1].on_changed(update)
sliders[2].on_changed(update)
sliders[3].on_changed(update)
sliders[4].on_changed(update)
sliders[5].on_changed(update)
sliders[6].on_changed(update)
sliders[7].on_changed(update)
sliders[8].on_changed(update)
sliders[9].on_changed(update)
sliders[10].on_changed(update)
sliders[11].on_changed(update)
sliders[12].on_changed(update)
sliders[13].on_changed(update)
sliders[14].on_changed(update)
sliders[15].on_changed(update)

multiplier_sliders[0].on_changed(update)
multiplier_sliders[1].on_changed(update)
multiplier_sliders[2].on_changed(update)
multiplier_sliders[3].on_changed(update)
multiplier_sliders[4].on_changed(update)
multiplier_sliders[5].on_changed(update)
multiplier_sliders[6].on_changed(update)
multiplier_sliders[7].on_changed(update)
multiplier_sliders[8].on_changed(update)
multiplier_sliders[9].on_changed(update)
multiplier_sliders[10].on_changed(update)
multiplier_sliders[11].on_changed(update)
multiplier_sliders[12].on_changed(update)
multiplier_sliders[13].on_changed(update)
multiplier_sliders[14].on_changed(update)
multiplier_sliders[15].on_changed(update)


fig1.set_tight_layout(False)
fig2.set_tight_layout(False)
plt.show()








