import sys
from enum import Enum
from pymoo.configuration import Configuration
from pymoo.factory import get_problem, get_reference_directions, get_performance_indicator
#from pymoo.visualization.scatter import Scatter

sys.path.append("C:/Users/Trifenix/Desktop/Python")
sys.path.append("C:/Users/Trifenix/Desktop/Python/Algorithms")
sys.path.append("C:/Users/Trifenix/Desktop/Python/Problems/CEC_2020")

import NSGA_II
import NSGA_III
from MMF1 import MMF1
from MMF2 import MMF2
from MMF4 import MMF4
from Optimizer import Optimizer

Configuration.show_compile_hint = False

Problems = {
        "MMF1": MMF1(),
        "MMF2": MMF2(),
        "MMF4": MMF4(),
        "ZDT1": get_problem("zdt1"),
        "DTLZ1": get_problem("dtlz1"),
}

class Algorithms(Enum):
    NSGAII = 1,
    NSGAIII = 2
    
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

problem = args.get("problem")
if(problem == None):
	sys.exit("Error: Falta el argumento <problem>.")
else:
    problem = problem.rstrip("\r\n")
    if (problem not in Problems):
        sys.exit(f"Error: El argumento <{problem}> no se encuentra registrado en el diccionario <Problems>.")
        
algorithm = args.get("algorithm")
if(algorithm == None):
	sys.exit("Error: Falta el argumento <algorithm>.")
else:
    algorithm = eval(f"Algorithms.{algorithm}")
    if(algorithm == None):
        sys.exit(f"Error: El argumento <algorithm> no se encuentra registrado en la enumeración <{Algorithms}>.")

pop_size = args.get("pop_size")
if(pop_size == None):
	sys.exit("Error: Falta el argumento <pop_size>.")
else:
	pop_size = int(pop_size)
	if(pop_size < 1):
		sys.exit("Error: El argumento <pop_size> debe ser mayor a cero.")

crossover_probability = args.get("crossover_probability")
if(crossover_probability == None):
	sys.exit("Error: Falta el argumento <crossover_probability>.")
else:
	crossover_probability = float(crossover_probability)
	if(crossover_probability < 0 or crossover_probability > 1):
		sys.exit("Error: El argumento <crossover_probability> debe estar dentro del intervalo [0,1].")
        
mutation_probability = args.get("mutation_probability")
if(mutation_probability != None):
	mutation_probability = float(mutation_probability)
	if(mutation_probability < 0 or mutation_probability > 1):
		sys.exit("Error: El argumento <mutation_probability> debe estar dentro del intervalo [0,1].")
        
n_partitions = args.get("n_partitions")
if(n_partitions == None):
	n_partitions = 12
else:
	n_partitions = int(n_partitions)
	if(n_partitions < 1):
		sys.exit("Error: El argumento <n_partitions> debe ser mayor a cero.")

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
        
def Main(algorithm, problem, pop_size, crossover_probability, mutation_probability, n_partitions, n_gen, seed):

    # Instancia el problema
    problem = Problems.get(problem)

    reference_directions = get_reference_directions("das-dennis", problem.n_obj, n_partitions = n_partitions)
    
    # Instancia el algoritmo
    algorithm = NSGA_II.Get_Algorithm_Instance(pop_size, crossover_probability, mutation_probability) if (algorithm == Algorithms.NSGAII) else NSGA_III.Get_Algorithm_Instance(reference_directions, pop_size, crossover_probability, mutation_probability) if (algorithm == Algorithms.NSGAIII) else None

    # Instancia el optimizador
    optimizer = Optimizer(problem, algorithm)

    optimization_result = optimizer.Minimize(n_gen, seed)
    objective_spaces_values = optimization_result.F

    pareto_front = problem.pareto_front(reference_directions) if type(problem).__name__ == "DTLZ1" else problem.pareto_front()
    
    # Instancia los indicadores de rendimiento (Distancia Generacional Invertida (IGD) / Distancia Generacional Invertida Plus (IGD+))
    IGD = get_performance_indicator("igd", pareto_front)
    #IGD_plus = get_performance_indicator("igd+", pareto_front)

    # Imprime las métricas obtenidas por el conjunto de soluciones resultantes de la optimización multimodal/multiobjetivo
    print("IGD:", IGD.calc(objective_spaces_values))
    #print("\n\tIGD:", IGD.calc(objective_spaces_values))
    #print("\tIGD+:", IGD_plus.calc(objective_spaces_values))

    # Grafica la frontera de Pareto y los valores resultantes de la optimización en el espacio objetivo.
    #Scatter(title = "Espacio objetivo", figsize=(10, 10)).add(pareto_front, color = "blue", facecolors = "white").add(objective_spaces_values, color = "navy").show()

#problem = "DTLZ1"
#algorithm = Algorithms.NSGAIII
#pop_size = 100
#crossover_probability = 0.5
#mutation_probability = None
#n_partitions = 12
#n_gen = 400
#seed = 1

#Ejecuta el script
Main(algorithm, problem, pop_size, crossover_probability, mutation_probability, n_partitions, n_gen, seed)