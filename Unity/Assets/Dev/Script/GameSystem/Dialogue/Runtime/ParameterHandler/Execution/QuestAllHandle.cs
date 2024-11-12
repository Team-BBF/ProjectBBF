using System;
using System.Collections.Generic;
using ProjectBBF.Persistence;
using UnityEngine;

namespace DS.Runtime
{
    [CreateAssetMenu(menuName = "ProjectBBF/Dialogue/Execution handler/QuestAllCancel", fileName = "New Quest All Cancel")]
    public class QuestAllCancelHandler : ParameterHandlerArgsT
    {
        protected object OnExecute()
        {
            foreach ((string questKey, QuestType state) tuple in QuestManager.Instance.GetAllQuestState())
            {
                if (tuple.state == QuestType.Create)
                {
                    QuestManager.Instance.ESO.Raise(new QuestEvent()
                    {
                        QuestKey = tuple.questKey,
                        Type = QuestType.Cancele
                    });
                }
            }
            
            return null;
        }
    }
}