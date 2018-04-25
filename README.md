# Hull-Delaunay-Voronoi

This project shows how to calculate the Convex Hull, Delaunay triangulation or Voronoi diagram from a random set of points. The project is based of the [MIConvex Hull](https://designengrlab.github.io/MIConvexHull/) code but I have restructured it a bit and extended the Delaunay and Voronoi parts.


The 3 concepts of Hull, Delaunay and Voronoi or all related. If you have a Convex Hull algorithm that will work for any dimension of space then you can use that to find the Delaunay triangulation using the 'lifting method'. From the Delaunay triangulation you can then find the Voronoi diagram as its the Delaunay's Dual Graph.


The code to calculate the convex hull may throw a exception. For example if a 2D hull tries to process a line that has two vertices in the same position or a 3D hull tries to process a triangle where all the points are co-planar (they form a line). These can not be processed so will result in a error. Randomizing the vertices order or adding a small amount of noise to the positions can solve this.

You can download a Unity package [here](https://app.box.com/s/7427ychbep6ck32xqveslo1qgi4hwvq3).

A Convex Hull from 2D points.

![2D Convex Hull](https://static.wixstatic.com/media/1e04d5_9901554eec594698bd4d45ecd862f250~mv2.jpg/v1/fill/w_550,h_550,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_9901554eec594698bd4d45ecd862f250~mv2.jpg)

A Convex Hull from 3D points.

![3D Convex Hull](https://static.wixstatic.com/media/1e04d5_212c6f5cd10942dc8ea872fa5460c449~mv2.jpg/v1/fill/w_550,h_550,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_212c6f5cd10942dc8ea872fa5460c449~mv2.jpg)

A Delaunay triangulation from 2D points.

![2D Delaunay triangulation](https://static.wixstatic.com/media/1e04d5_bb6ef781732042a09285c2fc2715bc09~mv2.jpg/v1/fill/w_550,h_550,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_bb6ef781732042a09285c2fc2715bc09~mv2.jpg)

A Voronoi diagram from 2D points.

![2D Voronoi diagram](https://static.wixstatic.com/media/1e04d5_51d9f0ebbe1b45edb1e9db9a75cba901~mv2.jpg/v1/fill/w_550,h_550,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_51d9f0ebbe1b45edb1e9db9a75cba901~mv2.jpg)

A Voronoi diagram from 3D points.

![3D Voronoi diagram](https://static.wixstatic.com/media/1e04d5_4f48d64b35774be4802b44407400df81~mv2.jpg/v1/fill/w_550,h_550,al_c,q_80,usm_0.66_1.00_0.01/1e04d5_4f48d64b35774be4802b44407400df81~mv2.jpg)
