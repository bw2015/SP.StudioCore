# SP.StudioCore

## Data 数据库操作
> 支持SQLServer/MySQL

  // 数据库操作示例
  public class DemoAgent : DbAgent<DemoAgent>{
    public DemoAgent():base("ConnectionString"){}
  
    public void Execute(string sql)}
      using(DbExecutor db = NewExecutor()){
        db.Execute(sql);
      }
    }
  }
