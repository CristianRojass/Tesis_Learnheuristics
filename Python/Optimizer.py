from pymoo.optimize import minimize

class Optimizer():

    def __init__(self, problem, algorithm):
        self.problem = problem
        self.algorithm = algorithm

    def Minimize(self, number_of_generations = 500, seed = 1):
        return minimize(self.problem, self.algorithm, ('n_gen', number_of_generations), seed = seed)