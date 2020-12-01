import math
import numpy as np
from pymoo.model.problem import Problem

class MMF1(Problem):

    def __init__(self, **kwargs):
        super().__init__(n_var=2, n_obj=2, n_constr=0, xl=np.array([1, -1]), xu=np.array([3, 1]), **kwargs)

    def _calc_pareto_front(self, n_pareto_points = 100):
        pareto_points = np.linspace(0, 1, n_pareto_points)
        pareto_front = np.array([[f1, 1 - math.sqrt(f1)] for f1 in pareto_points])
        return pareto_front

    def f1(self, x_1):
        return abs(x_1 - 2)
    
    def f2(self, x_1, x_2):
        return 1 - math.sqrt(abs(x_1 - 2)) + 2 * math.pow(x_2 - math.sin(6 * math.pi * abs(x_1 - 2) + math.pi), 2)

    def _evaluate(self, x, out, *args, **kwargs):
        out["F"] = [[ self.f1(vector[0]), self.f2(vector[0], vector[1]) ] for vector in x]