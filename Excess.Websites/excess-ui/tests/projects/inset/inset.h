#pragma once

#include "../../geometry.h"
#include "../straightskeleton/straight_skeleton.h"

struct inset_labels
{
	int result_faces;
	int result_edges;
	int side_faces;
	int side_result_edges;
	int side_side_edges;

	void apply_result_face(int& label)
	{
		if (result_faces >= 0)
			label = result_faces;
	}

	bool modifies_result_edges()
	{
		return (side_result_edges >= 0) || (result_edges >= 0);
	}

	void apply_result_edge(geometry::edge2& eit)
	{
		if (side_result_edges >= 0)
			eit->twin()->data().label = side_result_edges;

		if (result_edges >= 0)
			eit->data().label = result_edges;
	}

	void apply_side_face(geometry::face2& f)
	{
		if (side_side_edges >= 0)
		{
			assert(false); //td: go over edges, determine a side face on the other side and apply
		}

		if (side_faces >= 0)
			f->data().label = side_faces;

	}
};

struct inset_request
{
	DEFAULT_CGAL_TYPES()
	DEFAULT_ARRANGEMENT_TYPES()

	inset_request(
		arrangement2& arr_,  
		face2& f_, 
		face2_list* faces_ = null, 
		face2_list* sides_ = null) :
		arr(arr_),
		f(f_),
		faces(faces_),
		sides(sides_)
	{
	}

	arrangement2& arr;
	face2& f;
	face2_list* faces;
	face2_list* sides;
};

struct inset
{
	DEFAULT_CGAL_TYPES()
	DEFAULT_ARRANGEMENT_TYPES()

	static void run(double amount, inset_request& request, inset_labels& labels);
};
