# NotchDelta
A Notch-Delta receptor binding process agent-based model using Unity 3D

The simulation was built and tested on Unity version 4.6.1, which is freely available for download at https://unity3d.com/get-unity/download/archive. To use the simulation, download the repository from GitHub, save it to a new folder (e.g. notch-delta-model), and create a project in Unity by clicking “create a new project” and select that folder (it should contain only the Assets folder and the readme file). Do not import any package. Then, load one of the scenes in the Assets folder by double clicking it. 

The following parameters are editable by clicking the object name on the object browser in unity (other parameters should not be changed to avoid unwanted behavior): 
* On the SimulationManager object: 
  * Length of the experiment in seconds
  * Diffusion speed of membranal proteins (both receptors and ligands)
  * Binding probability of receptor-ligand pairs
  * Unbinding probability of receptor-ligand complex
  * Signal generation probability of receptor ligand complex
  * Binding distance
  * Interaction radius
  * Endocytosis rate (probability of endocytosing per receptor per second)
* On each Cell (referred to as cell1Delta and cell2Notch) object:
  * Random seed for receptor movement
  * Exocytosis rate (number of membranal proteins per second)
  * Percent of receptors to show – note that this depends heavily on the strength of hardware available. Lower value if the simulation stutters.
  
For the Filopodia model, the following parameters may be edited on the filopodial cell:
* Exocytosis mainly in filopodia – toggle to enable this option.
* Ratio of exocytosing receptors in filopodia – the percent of receptors to exocytose in the filopodium. 

To run an experiment, click the play button. The camera can be moved using the WSADQE keyboard keys, and the pointing direction is controlled by mouse movement. Note that on lower-end computers, it may take up to a minute for the simulation to start. 
