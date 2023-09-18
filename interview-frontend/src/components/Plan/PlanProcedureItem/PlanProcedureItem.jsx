import React, { useEffect, useState } from "react";
import ReactSelect from "react-select";

import {
    addUserToPlanProceduer,
    getAssignedProcedureUsers
  } from "../../../api/api";

const PlanProcedureItem = ({ procedure, users, planId }) => {
    const [selectedUsers, setSelectedUsers] = useState(null);

    useEffect(() => {
        const fetchAssignedUers = async () => {
            const assignedUsers = await getAssignedProcedureUsers(planId, procedure.procedureId);
            const persistedAssignedUsers = assignedUsers.map(user => {
                const [first] = users.filter(u => u.value === user.userId);
                return first; 
            });
            console.log(persistedAssignedUsers);
            setSelectedUsers(persistedAssignedUsers);
        };
        fetchAssignedUers();
    }, []);

    const handleAssignUserToProcedure = async (e) => {
        setSelectedUsers(e);
        const arrUserIds = e.map(user => user.value);
        await addUserToPlanProceduer(planId, procedure.procedureId, arrUserIds);
    };

    return (
        <div className="py-2">
            <div>
                {procedure.procedureTitle}
            </div>

            <ReactSelect
                className="mt-2"
                placeholder="Select User to Assign"
                isMulti={true}
                options={users}
                value={selectedUsers}
                onChange={(e) => handleAssignUserToProcedure(e)}
            />
        </div>
    );
};

export default PlanProcedureItem;
