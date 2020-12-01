import sys

# Recibo los argumentos a través del StandardInput, es un lista de lineas compuestas por strings que siguen el siguiente patrón: <nombre_de_argumento:valor_argumento>,
# donde nombre_de_argumento es de tipo string ya que será la clave del diccionario, y valor_argumento es una representación en string de un valor como: {83,12.6,false,true,[(1,2),(8,3),(5,7)]}

# Convierte la lista de argumentos en un diccionario
args = {}
for arg in sys.stdin: 
	if 'q' == arg.rstrip(): 
		break
	key_value_arg = arg.split(":")
	arg_key = key_value_arg[0]
	arg_value = key_value_arg[1]
	args.setdefault(arg_key,arg_value)

import matplotlib.pyplot as plt
import ast
import os

#Grafica los puntos y guarda una representación en imagen (png) 
def Plot(x, y, z, black_hole, hypercube, epoch, degrees_tuples_rotation):
	path_to_folder_container = f"C:\\Users\\Trifenix\\Desktop\\Tesis_Learnheuristics\\HyperSpace\\{hypercube}"
	if(not os.path.exists(path_to_folder_container)):
		os.makedirs(path_to_folder_container)
	if(z == None):														#2D
		fig = plt.figure()
		ax = fig.add_subplot()
		ax.set(xlabel="R", ylabel="R²")
		ax.scatter(x, y, marker='o', color='darkgray')
		for x,y in zip(x,y):
			label = f"({x},{y})"
			plt.annotate(label, # this is the text
				(x,y), # this is the point to label
				textcoords="offset points", # how to position the text
				xytext=(0,-10), # distance from text to points (x,y)
				ha='center', # horizontal alignment can be left, right or center
				fontsize=8)
			if(black_hole != None):
				plt.annotate("Black hole", # this is the text
				(black_hole[0],black_hole[1]), # this is the point to label
				textcoords="offset points", # how to position the text
				xytext=(0,5), # distance from text to points (x,y)
				ha='center', # horizontal alignment can be left, right or center
				fontsize=8)
				plt.plot(black_hole[0], black_hole[1], marker='o', color='black')
		fig.savefig(f"{path_to_folder_container}\\Scatter2D - (Epoch_{epoch}).png")
	else:															#3D
		fig, axs = plt.subplots(2, 3, figsize=(15,10), subplot_kw={'projection': '3d'})
		axs[0, 0].set_title('<Horizontal:-45°, Vertical:0°>')
		axs[0, 0].view_init(0,-45)
		axs[0, 0].scatter(x, y, z, marker='o', color='darkgray')
		axs[0, 1].set_title('<Horizontal:45°, Vertical:45°>')
		axs[0, 1].view_init(45,45)
		axs[0, 1].scatter(x, y, z, marker='o', color='darkgray')
		axs[0, 2].set_title('<Horizontal:45°, Vertical:0°>')
		axs[0, 2].view_init(0,45)
		axs[0, 2].scatter(x, y, z, marker='o', color='darkgray')
		axs[1, 0].set_title('<Horizontal:0°, Vertical:0°>')
		axs[1, 0].view_init(0,0)
		axs[1, 0].set_xticklabels([])
		axs[1, 0].scatter(x, y, z, marker='o', color='darkgray')
		axs[1, 1].set_title('<Horizontal:90°, Vertical:0°>')
		axs[1, 1].view_init(0,90)
		axs[1, 1].set_yticklabels([])
		axs[1, 1].scatter(x, y, z, marker='o', color='darkgray')
		axs[1, 2].set_title('<Horizontal:0°, Vertical:90°>')
		axs[1, 2].view_init(90,0)
		axs[1, 2].set_zticklabels([])
		axs[1, 2].scatter(x, y, z, marker='o', color='darkgray')

		for ax in axs.flat:
			ax.set(xlabel="R", ylabel="R²", zlabel="R³")
			if(black_hole != None):
				label = f"({black_hole[0]},{black_hole[1]},{black_hole[2]})"
				ax.text(black_hole[0], black_hole[1], black_hole[2], f"Black Hole {label}", color='black', fontsize = 8)
				ax.plot(black_hole[0], black_hole[1], black_hole[2], marker='o', color='black')
		fig.savefig(f"{path_to_folder_container}\\Scatter3D - (Epoch_{epoch}).png")

		#Si están asignados los valores de rotación crea una gráfica personalizada
		if(degrees_tuples_rotation != None):
			columns = len(degrees_tuples_rotation) if len(degrees_tuples_rotation) < 4 else 2
			rows = 1 + int(len(degrees_tuples_rotation)/4)
			fig, axs = plt.subplots(rows, columns, figsize=(columns*5,rows*5), subplot_kw={'projection': '3d'})
			list_plot = [ axs ] if len(degrees_tuples_rotation) == 1 else axs.flat
			for index, ax in enumerate(list_plot):
				horizontal_rotation = degrees_tuples_rotation[index][0].replace('\'','')
				vertical_rotation = degrees_tuples_rotation[index][1].replace('\'','')
				ax.set(title=f"<Horizontal:{horizontal_rotation}°, Vertical:{vertical_rotation}°>", xlabel="R", ylabel="R²", zlabel="R³")
				ax.view_init(float(vertical_rotation),float(horizontal_rotation))
				ax.scatter(x, y, z, marker='o')
			fig.savefig(f"{path_to_folder_container}\\Custom_Scatter3D - (Epoch_{epoch}).png")	

#Abre una ventana para mostrar los gráficos
def OpenImage(path):
	os.startfile(path)

#Ejecuta las funciones
def Main(x, y, z, black_hole, hypercube, epoch, degrees_tuples_rotation, open_image):
	Plot(x, y, z, black_hole, hypercube, epoch, degrees_tuples_rotation)
	if(open_image == True):
		title = "Scatter2D" if z == None else "Scatter3D" if degrees_tuples_rotation == None else "Custom_Scatter3D"
		title += f" - (Epoch_{epoch})"
		path = f"C:\\Users\\Trifenix\\Desktop\\Tesis_Learnheuristics\\HyperSpace\\{hypercube}\\{title}.png"
		OpenImage(path)

#Mapea los argumentos de linea de comandos a variables
x = args.get("x")
if(x == None):
	sys.exit("Error: Falta el argumento <x>.")
else:
	x = ast.literal_eval(x)
y = args.get("y")
if(y == None):
	sys.exit("Error: Falta el argumento <y>.")
else:
	y = ast.literal_eval(y)
z = args.get("z")
if(z != None):
	z = ast.literal_eval(z)
black_hole = args.get("black_hole")
if(black_hole != None):
	black_hole = [float(coordinate) for coordinate in black_hole.replace('(','').replace(')','').split(',')]
hypercube_name = args.get("hypercube_name")
if(hypercube_name == None):
	sys.exit("Error: Falta el argumento <hypercube_name>.")
hypercube_name = hypercube_name.strip('\n')
epoch = args.get("epoch")
if(epoch == None):
	sys.exit("Error: Falta el argumento <epoch>.")
else:
	epoch = int(epoch)
	if(epoch < 0):
		sys.exit("Error: El argumento <epoch> debe ser mayor o igual a cero.")
degrees_rotation = args.get("degrees_rotation")
degrees_tuples_rotation = None
if(degrees_rotation != None):
	degrees_sequence_rotation = degrees_rotation.replace('[','').replace(']','').replace('(','').replace(')','').split(',')
	degrees_tuples_rotation = list(zip(degrees_sequence_rotation[::2], degrees_sequence_rotation[1::2]))
	if(len(degrees_tuples_rotation) > 4):
		sys.exit("Error: Solo se puede configurar un maximo de 4 rotaciones personalizadas para Custom_Scatter3D.")
open_image = eval(args.get("open_image", "False"))

#Ejecuta el script
Main(x, y, z, black_hole, hypercube_name, epoch, degrees_tuples_rotation, open_image)