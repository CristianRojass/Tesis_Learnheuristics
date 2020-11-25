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

from pymoo.configuration import Configuration
from pymoo.algorithms.nsga3 import NSGA3
from pymoo.factory import get_problem, get_reference_directions, get_performance_indicator
from pymoo.optimize import minimize
#from pymoo.visualization.scatter import Scatter

Configuration.show_compile_hint = False

def Main(n_partitions, pop_size, n_gen, seed):

	# Crea las direcciones de referencia que se utilizarán para la optimización
	ref_dirs = get_reference_directions("das-dennis", 3, n_partitions = n_partitions)

	# Instancia el algoritmo
	algorithm = NSGA3(pop_size = pop_size, ref_dirs = ref_dirs)

	# Instancia el problema
	problem = get_problem("dtlz1")

	# Obtiene el frente de pareto verdadero
	pareto_front = problem.pareto_front(ref_dirs)

	# Instancia el indicador de rendimiento (Distancia Generacional Inversa/IGD)
	IGD = get_performance_indicator("igd", pareto_front)

	# Ejecuta la optimización
	res = minimize(problem, algorithm, seed=seed, termination=('n_gen', n_gen))

	# Imprime la IGD obtenida por el conjunto de soluciones resultantes de la optimización multimodal/multiobjetivo
	print("IGD>", IGD.calc(res.F))

	# Grafica los resultados obtenidos por la metaheurística
	#Scatter(title = "Valores obtenidos en espacio de decisión").add(res.X, color="green").show()
	#Scatter(title = "Valores obtenidos en espacio objetivo").add(res.F, color="red").show()
	#Scatter(title = "Frente de Pareto").add(pareto_front, color="blue").show()

	# Imprime los resultados obtenidos por la metaheurística
	#print("Valores obtenidos en espacio de decision", res.X)
	#print("Valores obtenidos en espacio objetivo", res.F)
	#print("Frente de Pareto", pareto_front)

n_partitions = args.get("n_partitions")
if(n_partitions == None):
	sys.exit("Error: Falta el argumento <n_partitions>.")
else:
	n_partitions = int(n_partitions)
	if(n_partitions < 1):
		sys.exit("Error: El argumento <n_partitions> debe ser mayor a cero.")

pop_size = args.get("pop_size")
if(pop_size == None):
	sys.exit("Error: Falta el argumento <pop_size>.")
else:
	pop_size = int(pop_size)
	if(pop_size < 1):
		sys.exit("Error: El argumento <pop_size> debe ser mayor a cero.")

n_gen = args.get("n_gen")
if(n_gen == None):
	sys.exit("Error: Falta el argumento <n_gen>.")
else:
	n_gen = int(n_gen)
	if(n_gen < 1):
		sys.exit("Error: El argumento <n_gen> debe ser mayor a cero.")
	
seed = args.get("seed")
if(seed == None):
	sys.exit("Error: Falta el argumento <seed>.")
else:
	seed = int(seed)
	if(seed < 1):
		sys.exit("Error: El argumento <seed> debe ser mayor a cero.")

#Ejecuta el script
Main(n_partitions, pop_size, n_gen, seed)