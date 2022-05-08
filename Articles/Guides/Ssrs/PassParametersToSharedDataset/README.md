# Abstract

This article describes how to pass report parameters to shared dataset in SQL Server Reporting Services project.

# Instructions

1. Create shared dataset with desired parameters;
2. `Optional` Prepare proper SQL query that uses parameters;
3. Add desired parameters in your report. It's good practice to provide the same name as the parameter in shared dataset;
4. Add previously created shared dataset to report;
5. Provide parameters in `Parameters` tab:

   * `Parameter name` - name of the parameter in shared dataset;
   * `Parameter value` - parameter from our report;

   For example:


| Parameter Name | Parameter Value |
| ---------------- | ----------------- |
| @PARAMETER_1 | [@PARAMETER_1] |

6. If you follow the example, it's all done;