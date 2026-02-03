# 导入必要的库
import pandas as pd
import numpy as np
import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

def save_fig(df):
    # ---------------------- 步骤2：数据重塑（关键：构造三维曲面图所需的二维网格和z矩阵） ----------------------
    # 1. 提取唯一的time和wavelength值（作为网格的坐标轴）
    unique_times = np.array(df['time'])  # 形状：(100,)
    unique_wavelengths = np.array(df.loc[0, 'wavelength'], dtype=np.float64)  # 形状：(999,)
    total_counts = np.array([df.loc[i, 'counts'] for i in range(df['counts'].shape[0])], dtype=np.float64)
    print(total_counts.shape)

    # 2. 构造二维网格（x=wavelength，y=time）
    # meshgrid返回二维数组，形状分别为 (100, 999)（与z轴匹配）
    X, Y = np.meshgrid(unique_wavelengths, unique_times)

    # 3. 重塑counts为二维矩阵（z轴），形状要求与X、Y一致：(100, 999)
    # 方法：将一维counts数据按每组999个拆分，组成100行999列的二维数组
    Z = total_counts.reshape(unique_times.shape[0], unique_wavelengths.shape[0])

    # ---------------------- 步骤3：绘制三维曲面图 ----------------------
    # 创建画布和3D坐标轴
    fig = plt.figure(figsize=(12, 8))
    ax = fig.add_subplot(111, projection='3d')

    # 绘制三维曲面（cmap指定颜色映射，alpha设置透明度）
    surf = ax.plot_surface(
        X, Y, Z,
        cmap='viridis',  # 常用颜色映射，可替换为jet、plasma等
        alpha=0.8,
        linewidth=0.1,  # 曲面网格线宽度
        antialiased=True  # 抗锯齿，让图像更平滑
    )

    # ---------------------- 步骤4：添加图表美化和标签 ----------------------
    # 设置坐标轴标签
    ax.set_xlabel('Wavelength', fontsize=12, labelpad=10)
    ax.set_ylabel('Time', fontsize=12, labelpad=10)
    ax.set_zlabel('Counts', fontsize=12, labelpad=10)

    # 设置图表标题
    ax.set_title('3D Surface Plot of Counts vs Wavelength & Time', fontsize=14, pad=20)

    # 添加颜色条（展示z轴counts的颜色对应关系）
    fig.colorbar(surf, ax=ax, shrink=0.8, aspect=20, label='Counts')

    # 调整视角（azim为方位角，elev为仰角，可自行调整）
    ax.view_init(elev=30, azim=45)

    # 显示图表
    plt.tight_layout()
    plt.savefig("plot.png")